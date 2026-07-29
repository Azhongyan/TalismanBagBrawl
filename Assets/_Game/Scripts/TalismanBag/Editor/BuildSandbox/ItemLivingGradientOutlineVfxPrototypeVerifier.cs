using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.BuildSandbox
{
    public static class ItemLivingGradientOutlineVfxPrototypeVerifier
    {
        private const string RuntimeRoot =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/";
        private const string ResourceRoot =
            "Assets/_Game/Resources/V04/"
            + "ItemLivingGradientOutlineDevOnly/";
        private const string ShaderPath =
            RuntimeRoot + "UI_ItemLivingGradientOutline.shader";
        private const string ProfilePath =
            ResourceRoot
            + "item_living_gradient_outline_profile.json";
        private const string PassMarker =
            "ITEM_LIVING_GRADIENT_OUTLINE_STATIC_QA_PASS";
        private const string RenderedPassMarker =
            "ITEM_LIVING_GRADIENT_OUTLINE_RENDERED_QA_PASS";
        private const string ScenePath =
            "Assets/_Game/Scenes/"
            + "Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string RenderedPendingKey =
            "ItemLivingGradientOutline.Rendered.Pending";
        private const string RenderedCaptureDirectoryKey =
            "ItemLivingGradientOutline.Rendered.CaptureDirectory";
        private const string RenderedSceneHashKey =
            "ItemLivingGradientOutline.Rendered.SceneHash";
        private const string RenderedResultReadyKey =
            "ItemLivingGradientOutline.Rendered.ResultReady";
        private const string RenderedPassedKey =
            "ItemLivingGradientOutline.Rendered.Passed";
        private const string RenderedEvidenceKey =
            "ItemLivingGradientOutline.Rendered.Evidence";

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes =
                new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    {
                        "Assets/_Game/Scenes/"
                        + "Scene_TalismanBag_V04_BattleSandboxPreview"
                        + ".unity",
                        "BE7E9536573CF7708F4D64961661C1FD"
                        + "60DD275F969E2B7A003A045B0D07306E"
                    },
                    {
                        RuntimeRoot
                        + "LiHuoCombatFeedbackOrchestrationController.cs",
                        "AB09DEEAA8504CDA7D38C43CF81BA9612"
                        + "ADF72A4CBEFD0E650AD726D4680A541"
                    },
                    {
                        "Assets/_Game/Scripts/TalismanBag/Editor/"
                        + "BuildSandbox/"
                        + "LiHuoCombatFeedbackOrchestrationVerifier.cs",
                        "92F81E4E5EE0A341A9BE1621841C06AA"
                        + "56B92A1630F3504FB2DC939F3224B211"
                    },
                    {
                        RuntimeRoot + "BuildItemPreviewCardView.cs",
                        "448A69762EC157214809E1D43CD19A134"
                        + "D77F7928742CC890A94A41B1588D56E"
                    },
                    {
                        RuntimeRoot
                        + "BuildGridInteractionPreviewController.cs",
                        "519A7B69FC16A9C50563CEB34F614967A"
                        + "E58854C57F4846D82BDCDEE9093523A"
                    },
                    {
                        RuntimeRoot
                        + "BattleSandboxItemTriggerFeedbackController.cs",
                        "665ED1DFFD39B99EEC9956FF29D5B5B2"
                        + "5632020C25F2A6DBBF8A81BFFD2F3789"
                    }
                };

        [MenuItem(
            "TalismanBag/V0.4/"
            + "Verify Item Living Gradient Outline VFX Prototype")]
        public static void VerifyFromMenu()
        {
            VerifyOrThrow();
        }

        [InitializeOnLoadMethod]
        private static void InitializeRenderedQaLifecycle()
        {
            RegisterRenderedQaLifecycle();
            string captureDirectory = ResolveArgument(
                "-itemLivingOutlineCaptureDir",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "符箓",
                    "QA_Evidence",
                    ItemLivingGradientOutlineProfile.PackageId));
            string rerunFlag = Path.Combine(
                captureDirectory,
                "rerun_requested.flag");
            if (File.Exists(rerunFlag))
            {
                File.Delete(rerunFlag);
                EditorApplication.delayCall += RunRenderedQa;
            }
            if (SessionState.GetBool(
                    RenderedPendingKey,
                    false))
            {
                EditorApplication.delayCall -=
                    EnsureRenderedDriver;
                EditorApplication.delayCall +=
                    EnsureRenderedDriver;
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/"
            + "Run Item Living Gradient Outline Rendered QA")]
        public static void RunRenderedQa()
        {
            VerifyOrThrow();
            if (Application.isBatchMode)
            {
                throw new InvalidOperationException(
                    "Rendered QA requires a normal graphics Editor.");
            }
            if (SessionState.GetBool(
                    RenderedPendingKey,
                    false))
            {
                return;
            }

            string captureDirectory = ResolveArgument(
                "-itemLivingOutlineCaptureDir",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "符箓",
                    "QA_Evidence",
                    ItemLivingGradientOutlineProfile.PackageId));
            Directory.CreateDirectory(captureDirectory);
            SessionState.SetBool(RenderedPendingKey, true);
            SessionState.SetString(
                RenderedCaptureDirectoryKey,
                captureDirectory);
            SessionState.SetString(
                RenderedSceneHashKey,
                ComputeSha256(ScenePath));
            SessionState.SetBool(
                RenderedResultReadyKey,
                false);
            SessionState.SetBool(RenderedPassedKey, false);
            SessionState.SetString(
                RenderedEvidenceKey,
                string.Empty);
            RegisterRenderedQaLifecycle();
            EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            Debug.Log(
                "["
                + ItemLivingGradientOutlineProfile.PackageId
                + "] RENDERED_QA_START capture="
                + captureDirectory);
            EditorApplication.isPlaying = true;
        }

        internal static void StoreRenderedDriverResult(
            bool passed,
            string evidence)
        {
            SessionState.SetBool(
                RenderedResultReadyKey,
                true);
            SessionState.SetBool(RenderedPassedKey, passed);
            SessionState.SetString(
                RenderedEvidenceKey,
                evidence ?? string.Empty);
        }

        public static void RunStaticBatch()
        {
            VerifyOrThrow();
        }

        public static void VerifyOrThrow()
        {
            List<string> failures = new();
            VerifyProtectedHashes(failures);
            VerifyAssetsAndProfile(failures);
            VerifyShaderContract(failures);
            VerifyAcceptedEventGate(failures);
            VerifyPoolAndSourceInvariants(failures);

            if (failures.Count > 0)
            {
                foreach (string failure in failures)
                {
                    Debug.LogError(
                        "["
                        + ItemLivingGradientOutlineProfile.PackageId
                        + "] STATIC_QA_FAIL " + failure);
                }
                throw new InvalidOperationException(
                    "Item living gradient outline QA failed: "
                    + string.Join(" | ", failures));
            }

            Debug.Log(
                "["
                + ItemLivingGradientOutlineProfile.PackageId
                + "] " + PassMarker
                + " alphaSamples=8"
                + " sharedMaterials=14"
                + " stressItems=30"
                + " acceptedTriggers=24"
                + " activeCap=6"
                + " sourceMutation=false"
                + " raycastBlock=false"
                + " androidStatic=true"
                + " protectedHashes=true"
                + " sceneSave=0");
        }

        private static void VerifyProtectedHashes(
            ICollection<string> failures)
        {
            foreach (KeyValuePair<string, string> pair in
                     ProtectedHashes)
            {
                if (!File.Exists(pair.Key))
                {
                    failures.Add(
                        "protected_missing=" + pair.Key);
                    continue;
                }
                string actual = ComputeSha256(pair.Key);
                if (!string.Equals(
                        actual,
                        pair.Value,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        "protected_hash_mismatch=" + pair.Key
                        + " expected=" + pair.Value
                        + " actual=" + actual);
                }
            }
        }

        private static void VerifyAssetsAndProfile(
            ICollection<string> failures)
        {
            string[] required =
            {
                ProfilePath,
                ShaderPath,
                ResourceRoot
                    + "UI_ItemLivingGradientOutline_LiHuo_Persistent.mat",
                ResourceRoot
                    + "UI_ItemLivingGradientOutline_LiHuo_Trigger.mat",
                ResourceRoot
                    + "UI_ItemLivingGradientOutline_TaiBai_Persistent.mat",
                ResourceRoot
                    + "UI_ItemLivingGradientOutline_TaiBai_Trigger.mat"
            };
            foreach (string path in required)
            {
                if (!File.Exists(path))
                {
                    failures.Add("asset_missing=" + path);
                }
            }

            TextAsset profileAsset =
                AssetDatabase.LoadAssetAtPath<TextAsset>(ProfilePath);
            ItemLivingGradientOutlineProfile profile =
                profileAsset == null
                    ? null
                    : JsonUtility.FromJson<
                        ItemLivingGradientOutlineProfile>(
                        profileAsset.text);
            if (profile == null
                || !string.Equals(
                    profile.previewTargetItemId,
                    "I009",
                    StringComparison.Ordinal)
                || profile.activeEffectCap != 6
                || Mathf.Abs(
                    profile.triggerAttackSeconds - 0.08f) > 0.001f
                || Mathf.Abs(
                    profile.triggerSpreadSeconds - 0.25f) > 0.001f
                || Mathf.Abs(
                    profile.triggerSettleSeconds - 0.55f) > 0.001f)
            {
                failures.Add("profile_timing_or_cap=invalid");
                return;
            }

            if (profile.ResolveBuildFamily("lihuo")
                    != ItemLivingGradientOutlineBuildFamily.LiHuo
                || profile.ResolveBuildFamily("taibai")
                    != ItemLivingGradientOutlineBuildFamily
                        .TaiBaiReserved
                || profile.ResolveBuildFamily("I009")
                    != ItemLivingGradientOutlineBuildFamily.Unknown
                || profile.liHuo == null
                || profile.taiBaiReserved == null)
            {
                failures.Add("palette_family_routing=invalid");
            }

            string[] materialPaths =
                required.Skip(2).ToArray();
            foreach (string path in materialPaths)
            {
                Material material =
                    AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null
                    || material.shader == null
                    || !string.Equals(
                        material.shader.name,
                        ItemLivingGradientOutlineProfile.ShaderName,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        "material_shader_binding=invalid:" + path);
                }
            }
        }

        private static void VerifyShaderContract(
            ICollection<string> failures)
        {
            if (!File.Exists(ShaderPath))
            {
                return;
            }
            string text = File.ReadAllText(ShaderPath);
            string[] requiredTokens =
            {
                "_MainTex_TexelSize",
                "outerBand",
                "maxNeighbour - centerAlpha",
                "atan2",
                "_FlowSpeed",
                "_GradientLut",
                "_NoiseTexture",
                "_RingTexture",
                "Stencil",
                "_ClipRect",
                "UNITY_UI_ALPHACLIP",
                "unity_GUIZTestMode",
                "#pragma target 2.0",
                "Blend SrcAlpha OneMinusSrcAlpha"
            };
            foreach (string token in requiredTokens)
            {
                if (text.IndexOf(
                        token,
                        StringComparison.Ordinal) < 0)
                {
                    failures.Add(
                        "shader_token_missing=" + token);
                }
            }
            for (int index = 0; index < 8; index++)
            {
                if (text.IndexOf(
                        "fixed n" + index + " = SampleAlpha",
                        StringComparison.Ordinal) < 0)
                {
                    failures.Add(
                        "alpha_neighbor_missing=n" + index);
                }
            }
            if (text.IndexOf(
                    "fixed n8",
                    StringComparison.Ordinal) >= 0
                || text.IndexOf(
                    "GrabPass",
                    StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf(
                    "Bloom",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                failures.Add(
                    "shader_mobile_budget_or_bloom=invalid");
            }
        }

        private static void VerifyAcceptedEventGate(
            ICollection<string> failures)
        {
            ItemLivingGradientOutlineAcceptedEventGate gate = new();
            gate.Reset(7);
            BattleSandboxAcceptedItemPresentationEvent last = null;
            for (int index = 0; index < 24; index++)
            {
                last = NewAcceptedPresentation(
                    7,
                    1500L + index * 1500L,
                    "accepted.living."
                    + index.ToString(
                        "D2",
                        CultureInfo.InvariantCulture));
                if (!gate.TryAccept(last, 7))
                {
                    failures.Add(
                        "accepted_event_rejected=" + index);
                    return;
                }
            }

            BattleSandboxAcceptedItemPresentationEvent stale =
                NewAcceptedPresentation(
                    7,
                    100L,
                    "accepted.living.stale");
            BattleSandboxAcceptedItemPresentationEvent
                wrongGeneration =
                    NewAcceptedPresentation(
                        6,
                        39000L,
                        "accepted.living.old_generation");
            if (gate.AcceptedCount != 24
                || gate.TryAccept(last, 7)
                || gate.TryAccept(stale, 7)
                || gate.TryAccept(wrongGeneration, 7)
                || gate.RejectedCount != 3)
            {
                failures.Add(
                    "accepted_event_dedup_or_monotonic=invalid");
            }
        }

        private static void VerifyPoolAndSourceInvariants(
            ICollection<string> failures)
        {
            GameObject fixtureRoot = null;
            Texture2D texture = null;
            Sprite sprite = null;
            ItemLivingGradientOutlineMaterialLibrary library =
                new();
            try
            {
                ItemLivingGradientOutlineProfile profile =
                    ItemLivingGradientOutlineProfile.Load();
                if (!library.Initialize(profile)
                    || library.MaterialCount != 14
                    || !library.TryGet(
                        ItemLivingGradientOutlineBuildFamily.LiHuo,
                        out ItemLivingGradientOutlineMaterialSet
                            materials))
                {
                    failures.Add(
                        "shared_material_library=invalid");
                    return;
                }

                fixtureRoot = new GameObject(
                    "ItemLivingGradientOutlineVerifierFixture",
                    typeof(RectTransform),
                    typeof(Canvas));
                texture = CreateSilhouetteTexture();
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);

                List<Image> sources = new();
                for (int index = 0; index < 30; index++)
                {
                    GameObject sourceObject = new(
                        "AuthoritativeArtwork_"
                        + index.ToString(
                            "D2",
                            CultureInfo.InvariantCulture),
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image));
                    sourceObject.transform.SetParent(
                        fixtureRoot.transform,
                        false);
                    Image source =
                        sourceObject.GetComponent<Image>();
                    source.sprite = sprite;
                    source.color = new Color(
                        0.73f, 0.42f, 0.29f, 0.87f);
                    source.raycastTarget = index % 2 == 0;
                    source.preserveAspect = true;
                    sources.Add(source);
                }

                List<ItemLivingGradientOutlineVfx> pool = new();
                for (int index = 0;
                     index < profile.activeEffectCap;
                     index++)
                {
                    pool.Add(
                        ItemLivingGradientOutlineVfx.CreatePooled(
                            fixtureRoot.transform,
                            index));
                }
                int initialMaterialCount = library.MaterialCount;
                Material expectedPersistent =
                    materials.Persistent;
                Material expectedTrigger = materials.Trigger;
                Material expectedBody =
                    materials.WholeBodyEmission;
                Material expectedInnerGlow =
                    materials.WholeBodyInnerGlow;
                Material expectedOuterGlow =
                    materials.WholeBodyOuterGlow;
                Material expectedEnvironmentSpill =
                    materials.EnvironmentSpill;

                for (int index = 0; index < 30; index++)
                {
                    ItemLivingGradientOutlineVfx effect =
                        pool[index % pool.Count];
                    Image source = sources[index];
                    ItemLivingGradientOutlineSourceSnapshot snapshot =
                        new(source);
                    if (!effect.Bind(
                            source,
                            "I"
                            + index.ToString(
                                "D3",
                                CultureInfo.InvariantCulture),
                            index % 2 == 0 ? "Tray" : "Board",
                            ItemLivingGradientOutlineBuildFamily.LiHuo,
                            profile.liHuo,
                            materials))
                    {
                        failures.Add(
                            "pool_bind_failed=" + index);
                        return;
                    }

                    effect.SetPersistent(true);
                    for (int trigger = 0; trigger < 20; trigger++)
                    {
                        float start = trigger;
                        effect.PlayTrigger(start);
                        effect.EvaluateAt(start + 0.08f);
                        effect.EvaluateAt(start + 0.25f);
                        effect.EvaluateAt(start + 0.56f);
                    }
                    if (!effect.AddedGraphicsAreRaycastFree
                        || !effect.SourceStillExact
                        || !snapshot.IsExact()
                        || effect.PersistentSharedMaterial
                            != expectedPersistent
                        || effect.TriggerSharedMaterial
                            != expectedTrigger
                        || effect.WholeBodyEmissionSharedMaterial
                            != expectedBody
                        || effect.WholeBodyInnerGlowSharedMaterial
                            != expectedInnerGlow
                        || effect.WholeBodyOuterGlowSharedMaterial
                            != expectedOuterGlow
                        || effect.EnvironmentSpillSharedMaterial
                            != expectedEnvironmentSpill)
                    {
                        failures.Add(
                            "source_raycast_or_material_invariant="
                            + index);
                        return;
                    }
                    effect.ReleaseToPool(fixtureRoot.transform);
                    if (!snapshot.IsExact())
                    {
                        failures.Add(
                            "source_restore_invariant=" + index);
                        return;
                    }
                }

                int runtimeRoots =
                    fixtureRoot
                        .GetComponentsInChildren<
                            ItemLivingGradientOutlineVfx>(true)
                        .Length;
                int residualBound =
                    pool.Count(value => value != null
                        && value.IsBound);
                if (runtimeRoots != profile.activeEffectCap
                    || residualBound != 0
                    || library.MaterialCount != initialMaterialCount)
                {
                    failures.Add(
                        "stress_growth_or_residual=invalid"
                        + " roots=" + runtimeRoots
                        + " residual=" + residualBound
                        + " materials=" + library.MaterialCount);
                }
            }
            finally
            {
                library.Dispose();
                if (sprite != null)
                {
                    UnityEngine.Object.DestroyImmediate(sprite);
                }
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                if (fixtureRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(fixtureRoot);
                }
            }
        }

        private static BattleSandboxAcceptedItemPresentationEvent
            NewAcceptedPresentation(
                int generation,
                long battleTick,
                string pulseEventId)
        {
            return new BattleSandboxAcceptedItemPresentationEvent(
                pulseEventId,
                "nian.living",
                "ledger.living",
                generation,
                battleTick,
                "I009",
                "instance.I009",
                "placement.I009",
                new[] { new ItemShapeCell(0, 0) },
                new[] { new ItemShapeCell(1, 1) },
                4,
                2,
                12,
                6,
                6);
        }

        private static Texture2D CreateSilhouetteTexture()
        {
            const int size = 32;
            Texture2D texture = new(
                size,
                size,
                TextureFormat.RGBA32,
                false,
                true);
            Color32[] pixels = new Color32[size * size];
            Vector2 center = new(15.5f, 15.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance =
                        Vector2.Distance(
                            new Vector2(x, y),
                            center);
                    byte alpha = distance < 8f
                        ? (byte)255
                        : distance < 10f
                            ? (byte)Mathf.RoundToInt(
                                Mathf.InverseLerp(
                                    10f, 8f, distance)
                                * 255f)
                            : (byte)0;
                    pixels[y * size + x] =
                        new Color32(220, 90, 35, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            texture.name =
                "ItemLivingGradientOutlineVerifierSilhouette";
            return texture;
        }

        private static void RegisterRenderedQaLifecycle()
        {
            EditorApplication.playModeStateChanged -=
                OnRenderedPlayModeChanged;
            EditorApplication.playModeStateChanged +=
                OnRenderedPlayModeChanged;
        }

        private static void OnRenderedPlayModeChanged(
            PlayModeStateChange state)
        {
            if (!SessionState.GetBool(
                    RenderedPendingKey,
                    false))
            {
                return;
            }
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall -=
                    EnsureRenderedDriver;
                EditorApplication.delayCall +=
                    EnsureRenderedDriver;
                return;
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CompleteRenderedQa();
            }
        }

        private static void EnsureRenderedDriver()
        {
            if (!SessionState.GetBool(
                    RenderedPendingKey,
                    false)
                || !EditorApplication.isPlaying
                || UnityEngine.Object.FindObjectOfType<
                    ItemLivingGradientOutlineRenderedQaDriver>()
                    != null)
            {
                return;
            }

            GameObject driverObject = new(
                "ItemLivingGradientOutlineRenderedQaDriver");
            driverObject.hideFlags =
                HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            ItemLivingGradientOutlineRenderedQaDriver driver =
                driverObject.AddComponent<
                    ItemLivingGradientOutlineRenderedQaDriver>();
            driver.Begin(
                SessionState.GetString(
                    RenderedCaptureDirectoryKey,
                    string.Empty));
        }

        private static void CompleteRenderedQa()
        {
            SessionState.SetBool(RenderedPendingKey, false);
            EditorApplication.delayCall -= EnsureRenderedDriver;
            bool sceneStable =
                string.Equals(
                    SessionState.GetString(
                        RenderedSceneHashKey,
                        string.Empty),
                    ComputeSha256(ScenePath),
                    StringComparison.Ordinal)
                && !SceneManager.GetActiveScene().isDirty;
            bool resultReady =
                SessionState.GetBool(
                    RenderedResultReadyKey,
                    false);
            bool passed =
                resultReady
                && SessionState.GetBool(
                    RenderedPassedKey,
                    false)
                && sceneStable;
            string evidence =
                SessionState.GetString(
                    RenderedEvidenceKey,
                    "driver_result_missing");
            if (passed)
            {
                Debug.Log(
                    "["
                    + ItemLivingGradientOutlineProfile.PackageId
                    + "] " + RenderedPassMarker
                    + " sceneStable=true " + evidence);
            }
            else
            {
                Debug.LogError(
                    "["
                    + ItemLivingGradientOutlineProfile.PackageId
                    + "] RENDERED_QA_FAIL sceneStable="
                    + sceneStable
                    + " resultReady=" + resultReady
                    + " " + evidence);
            }

            SessionState.SetBool(
                RenderedResultReadyKey,
                false);
            SessionState.SetBool(RenderedPassedKey, false);
            SessionState.SetString(
                RenderedEvidenceKey,
                string.Empty);
            SessionState.SetString(
                RenderedCaptureDirectoryKey,
                string.Empty);
            SessionState.SetString(
                RenderedSceneHashKey,
                string.Empty);
            EditorApplication.delayCall += () =>
                EditorApplication.Exit(passed ? 0 : 1);
        }

        private static string ResolveArgument(
            string key,
            string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(
                        args[index],
                        key,
                        StringComparison.Ordinal))
                {
                    return Path.GetFullPath(args[index + 1]);
                }
            }
            return Path.GetFullPath(fallback);
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter
                .ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty);
        }
    }

    [DefaultExecutionOrder(900)]
    public sealed class ItemLivingGradientOutlineRenderedQaDriver :
        MonoBehaviour
    {
        private enum QaStage
        {
            WaitForRuntime = 0,
            PrepareOff = 1,
            WaitOffCapture = 2,
            PreparePersistentA = 3,
            WaitPersistentA = 4,
            PreparePersistentB = 5,
            WaitPersistentB = 6,
            WaitTrayReturn = 7,
            WaitTrayCapture = 8,
            WaitBoardRecommit = 9,
            WaitRealAcceptedTrigger = 10,
            PrepareTriggerPeak = 11,
            WaitTriggerPeak = 12,
            PrepareSettled = 13,
            WaitSettled = 14,
            WaitStressSettle = 15,
            WaitCleanup = 16
        }

        private readonly struct QaPlacement
        {
            public readonly string itemId;
            public readonly ItemShapeCell cell;
            public readonly ItemShapeRotation rotation;

            public QaPlacement(
                string itemId,
                ItemShapeCell cell,
                ItemShapeRotation rotation)
            {
                this.itemId = itemId;
                this.cell = cell;
                this.rotation = rotation;
            }
        }

        private string captureDirectory;
        private string offPath;
        private string persistentAPath;
        private string persistentBPath;
        private string trayReturnPath;
        private string triggerPeakPath;
        private string settledPath;
        private ShougunuPhase1BattleSandboxVerticalSliceRuntime
            runtime;
        private ItemLivingGradientOutlineVfxPrototypeController
            controller;
        private IItemSystemBattleSandboxBoardAuthority authority;
        private QaStage stage;
        private float beganAt;
        private float stageStartedAt;
        private int realAcceptedBeforeBattle;
        private int acceptedGateBeforeBattle;
        private int controlledPreviewBeforeStress;
        private int packageWarnings;
        private int packageErrors;
        private Image boardSourceBeforeReturn;
        private ItemLivingGradientOutlineSourceSnapshot
            finalBoardSourceSnapshot;
        private bool hasFinalBoardSourceSnapshot;
        private string realPulseEventId = string.Empty;
        private bool finishing;

        public void Begin(string targetDirectory)
        {
            captureDirectory = Path.GetFullPath(targetDirectory);
            Directory.CreateDirectory(captureDirectory);
            offPath = EvidencePath("outline_off.png");
            persistentAPath =
                EvidencePath("persistent_living_a.png");
            persistentBPath =
                EvidencePath("persistent_living_b.png");
            trayReturnPath =
                EvidencePath("tray_return_rebind.png");
            triggerPeakPath =
                EvidencePath("real_trigger_peak.png");
            settledPath =
                EvidencePath("trigger_settled.png");
            foreach (string path in RequiredEvidencePaths())
            {
                DeleteEvidence(path);
            }
            beganAt = Time.unscaledTime;
            stageStartedAt = beganAt;
            stage = QaStage.WaitForRuntime;
            Application.logMessageReceived += OnLog;
        }

        private void Update()
        {
            if (finishing)
            {
                return;
            }
            if (Time.unscaledTime - beganAt > 110f)
            {
                Finish(
                    false,
                    "timeout stage=" + stage
                    + " diagnostic="
                    + (controller?.LastDiagnostic
                        ?? "controller_missing"));
                return;
            }

            switch (stage)
            {
                case QaStage.WaitForRuntime:
                    UpdateWaitForRuntime();
                    break;
                case QaStage.PrepareOff:
                    UpdatePrepareOff();
                    break;
                case QaStage.WaitOffCapture:
                    UpdateWaitOffCapture();
                    break;
                case QaStage.PreparePersistentA:
                    UpdatePreparePersistentA();
                    break;
                case QaStage.WaitPersistentA:
                    UpdateWaitPersistentA();
                    break;
                case QaStage.PreparePersistentB:
                    UpdatePreparePersistentB();
                    break;
                case QaStage.WaitPersistentB:
                    UpdateWaitPersistentB();
                    break;
                case QaStage.WaitTrayReturn:
                    UpdateWaitTrayReturn();
                    break;
                case QaStage.WaitTrayCapture:
                    UpdateWaitTrayCapture();
                    break;
                case QaStage.WaitBoardRecommit:
                    UpdateWaitBoardRecommit();
                    break;
                case QaStage.WaitRealAcceptedTrigger:
                    UpdateWaitRealAcceptedTrigger();
                    break;
                case QaStage.PrepareTriggerPeak:
                    UpdatePrepareTriggerPeak();
                    break;
                case QaStage.WaitTriggerPeak:
                    UpdateWaitTriggerPeak();
                    break;
                case QaStage.PrepareSettled:
                    UpdatePrepareSettled();
                    break;
                case QaStage.WaitSettled:
                    UpdateWaitSettled();
                    break;
                case QaStage.WaitStressSettle:
                    UpdateWaitStressSettle();
                    break;
                case QaStage.WaitCleanup:
                    UpdateWaitCleanup();
                    break;
            }
        }

        private void UpdateWaitForRuntime()
        {
            runtime ??= FindObjectOfType<
                ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
            controller ??= FindObjectOfType<
                ItemLivingGradientOutlineVfxPrototypeController>();
            if (authority == null)
            {
                ItemSystemBattleSandboxBoardAdapter adapter =
                    Resources.FindObjectsOfTypeAll<
                            ItemSystemBattleSandboxBoardAdapter>()
                        .FirstOrDefault(value => value != null
                            && value.gameObject.scene
                                == gameObject.scene);
                authority = adapter?.Authority;
            }
            if (runtime == null
                || controller == null
                || authority == null
                || !controller.Initialized)
            {
                return;
            }

            Shader shader = Shader.Find(
                ItemLivingGradientOutlineProfile.ShaderName);
            if (shader == null
                || !shader.isSupported
                || controller.SharedMaterialCount != 14)
            {
                Finish(
                    false,
                    "shader_or_material_init=false shader="
                    + (shader == null ? "null" : shader.name)
                    + " supported="
                    + (shader != null && shader.isSupported)
                    + " materials="
                    + controller.SharedMaterialCount);
                return;
            }
            if (!TryCommitOfficialP3PlayFixture())
            {
                return;
            }
            SetStage(QaStage.PrepareOff);
        }

        private void UpdatePrepareOff()
        {
            if (StageAge < 0.8f)
            {
                return;
            }
            controller.SetOutlineEnabled(false);
            SetStage(QaStage.WaitOffCapture);
        }

        private void UpdateWaitOffCapture()
        {
            if (StageAge < 0.35f)
            {
                return;
            }
            if (!TryResolveBoardArtwork(out Image boardSource)
                || boardSource == null
                || controller.ActiveEffectCount != 0)
            {
                Finish(
                    false,
                    "outline_off_baseline=false"
                    + " active=" + controller.ActiveEffectCount
                    + " source="
                    + (boardSource == null
                        ? "null"
                        : boardSource.name));
                return;
            }
            ScreenCapture.CaptureScreenshot(offPath);
            SetStage(QaStage.PreparePersistentA);
        }

        private void UpdatePreparePersistentA()
        {
            if (!EvidenceReady(offPath))
            {
                return;
            }
            controller.SetOutlineEnabled(true);
            SetStage(QaStage.WaitPersistentA);
        }

        private void UpdateWaitPersistentA()
        {
            if (StageAge < 0.8f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (!ValidateLiveEffect(effect, out string failure))
            {
                Finish(
                    false,
                    "persistent_a=" + failure);
                return;
            }
            finalBoardSourceSnapshot =
                new ItemLivingGradientOutlineSourceSnapshot(
                    effect.SourceImage);
            hasFinalBoardSourceSnapshot = true;
            ScreenCapture.CaptureScreenshot(persistentAPath);
            SetStage(QaStage.PreparePersistentB);
        }

        private void UpdatePreparePersistentB()
        {
            if (!EvidenceReady(persistentAPath))
            {
                return;
            }
            SetStage(QaStage.WaitPersistentB);
        }

        private void UpdateWaitPersistentB()
        {
            if (StageAge < 1.15f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (!ValidateLiveEffect(effect, out string failure)
                || !finalBoardSourceSnapshot.IsExact())
            {
                Finish(
                    false,
                    "persistent_b=false"
                    + " failure=" + failure
                    + " sourceExact="
                    + finalBoardSourceSnapshot.IsExact());
                return;
            }
            ScreenCapture.CaptureScreenshot(persistentBPath);
            boardSourceBeforeReturn = effect.SourceImage;
            SetStage(QaStage.WaitTrayReturn);
        }

        private void UpdateWaitTrayReturn()
        {
            if (!EvidenceReady(persistentBPath))
            {
                return;
            }
            ItemSystemBattleSandboxBoardOperationResult result =
                authority.ReturnToTray("P_BOARD_I009");
            if (!result.Accepted)
            {
                Finish(
                    false,
                    "return_to_tray_rejected="
                    + result.DiagnosticCode);
                return;
            }
            SetStage(QaStage.WaitTrayCapture);
        }

        private void UpdateWaitTrayCapture()
        {
            if (StageAge < 0.85f)
            {
                return;
            }
            if (File.Exists(trayReturnPath))
            {
                if (!EvidenceReady(trayReturnPath))
                {
                    return;
                }
                CommitI009BackToBoard();
                return;
            }
            bool boardResidual =
                ResolveI009Effect("Board") != null;
            bool oldSourceResidual =
                boardSourceBeforeReturn != null
                && boardSourceBeforeReturn
                    .GetComponentsInChildren<
                        ItemLivingGradientOutlineVfx>(true)
                    .Any(value => value != null
                        && value.IsBound);
            if (boardResidual
                || oldSourceResidual
                || controller.ActiveEffectCount
                    > controller.ActiveEffectCap)
            {
                Finish(
                    false,
                    "return_rebind_residual=true"
                    + " board=" + boardResidual
                    + " oldSource=" + oldSourceResidual
                    + " active="
                    + controller.ActiveEffectCount);
                return;
            }

            if (TryResolveTrayArtwork(out Image traySource))
            {
                ItemLivingGradientOutlineVfx trayEffect =
                    ResolveI009Effect("Tray");
                BuildItemPreviewCardView expectedCard =
                    traySource.GetComponentInParent<
                        BuildItemPreviewCardView>();
                GameObject trayHit = null;
                bool packageHit = false;
                if (!ValidateLiveEffect(
                        trayEffect,
                        out string failure)
                    || !TryRaycast(
                        traySource,
                        out trayHit,
                        out packageHit)
                    || trayHit == null
                    || expectedCard == null
                    || trayHit.GetComponentInParent<
                            BuildItemPreviewCardView>()
                        != expectedCard
                    || packageHit)
                {
                    Finish(
                        false,
                        "tray_rebind_or_raycast=false "
                        + failure);
                    return;
                }
                ScreenCapture.CaptureScreenshot(trayReturnPath);
                return;
            }
            if (StageAge > 4f)
            {
                Finish(
                    false,
                    "tray_authoritative_artwork_not_visible");
            }
        }

        private void UpdateWaitBoardRecommit()
        {
            if (StageAge < 0.85f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (!ValidateLiveEffect(effect, out string failure)
                || ResolveI009Effect("Tray") != null)
            {
                Finish(
                    false,
                    "board_recommit=" + failure);
                return;
            }
            finalBoardSourceSnapshot =
                new ItemLivingGradientOutlineSourceSnapshot(
                    effect.SourceImage);
            hasFinalBoardSourceSnapshot = true;
            realAcceptedBeforeBattle =
                controller.RealAcceptedTriggerCount;
            acceptedGateBeforeBattle =
                controller.EventGateAcceptedCount;
            runtime.HandleAuthoredBattleStateButton();
            SetStage(QaStage.WaitRealAcceptedTrigger);
        }

        private void UpdateWaitRealAcceptedTrigger()
        {
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (controller.RealAcceptedTriggerCount
                    > realAcceptedBeforeBattle
                && effect != null
                && effect.IsTriggerActive)
            {
                realPulseEventId =
                    controller.LastAcceptedPulseEventId;
                SetStage(QaStage.PrepareTriggerPeak);
            }
        }

        private void UpdatePrepareTriggerPeak()
        {
            if (StageAge < 0.07f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (!ValidateLiveEffect(effect, out string failure)
                || !effect.IsTriggerActive)
            {
                Finish(
                    false,
                    "real_trigger_peak=" + failure
                    + " triggerActive="
                    + (effect != null
                        && effect.IsTriggerActive));
                return;
            }
            ScreenCapture.CaptureScreenshot(triggerPeakPath);
            SetStage(QaStage.WaitTriggerPeak);
        }

        private void UpdateWaitTriggerPeak()
        {
            if (!EvidenceReady(triggerPeakPath))
            {
                return;
            }
            SetStage(QaStage.PrepareSettled);
        }

        private void UpdatePrepareSettled()
        {
            if (StageAge < 0.7f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect =
                ResolveI009Effect("Board");
            if (!ValidateLiveEffect(effect, out string failure)
                || effect.IsTriggerActive
                || !effect.IsPersistent
                || !hasFinalBoardSourceSnapshot
                || !finalBoardSourceSnapshot.IsExact())
            {
                Finish(
                    false,
                    "trigger_settle=" + failure
                    + " triggerActive="
                    + (effect != null
                        && effect.IsTriggerActive)
                    + " persistent="
                    + (effect != null
                        && effect.IsPersistent));
                return;
            }
            ScreenCapture.CaptureScreenshot(settledPath);
            SetStage(QaStage.WaitSettled);
        }

        private void UpdateWaitSettled()
        {
            if (!EvidenceReady(settledPath))
            {
                return;
            }
            controlledPreviewBeforeStress =
                controller.ControlledPreviewTriggerCount;
            for (int index = 0; index < 20; index++)
            {
                if (!controller.PlayControlledTriggerPreview())
                {
                    Finish(
                        false,
                        "controlled_stress_trigger_rejected="
                        + index);
                    return;
                }
            }
            SetStage(QaStage.WaitStressSettle);
        }

        private void UpdateWaitStressSettle()
        {
            if (StageAge < 0.72f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx[] effects =
                Resources.FindObjectsOfTypeAll<
                        ItemLivingGradientOutlineVfx>()
                    .Where(value => value != null
                        && value.gameObject.scene
                            == gameObject.scene)
                    .ToArray();
            bool raycastFree =
                effects.All(value =>
                    value.AddedGraphicsAreRaycastFree);
            bool materialsStable =
                controller.SharedMaterialCount == 14;
            bool capStable =
                controller.ActiveEffectCount
                    <= controller.ActiveEffectCap
                && controller.RuntimeEffectRootCount
                    <= controller.ActiveEffectCap
                && controller.LifetimeEffectRootCount
                    <= controller.ActiveEffectCap;
            bool triggerStressExact =
                controller.ControlledPreviewTriggerCount
                    - controlledPreviewBeforeStress == 20;
            bool realAccepted =
                controller.RealAcceptedTriggerCount
                    > realAcceptedBeforeBattle
                && controller.EventGateAcceptedCount
                    > acceptedGateBeforeBattle
                && !string.IsNullOrWhiteSpace(
                    realPulseEventId);
            bool sourceExact =
                hasFinalBoardSourceSnapshot
                && finalBoardSourceSnapshot.IsExact();
            bool screenshotsReady =
                RequiredEvidencePaths()
                    .All(EvidenceReady);
            if (!raycastFree
                || !materialsStable
                || !capStable
                || !triggerStressExact
                || !realAccepted
                || !sourceExact
                || !screenshotsReady
                || packageWarnings != 0
                || packageErrors != 0)
            {
                Finish(
                    false,
                    "stress_or_invariants=false"
                    + " raycast=" + raycastFree
                    + " materials=" + materialsStable
                    + " cap=" + capStable
                    + " trigger20=" + triggerStressExact
                    + " real=" + realAccepted
                    + " source=" + sourceExact
                    + " screenshots=" + screenshotsReady
                    + " warnings=" + packageWarnings
                    + " errors=" + packageErrors);
                return;
            }
            controller.SetOutlineEnabled(false);
            SetStage(QaStage.WaitCleanup);
        }

        private void UpdateWaitCleanup()
        {
            if (StageAge < 0.35f)
            {
                return;
            }
            ItemLivingGradientOutlineVfx[] effects =
                Resources.FindObjectsOfTypeAll<
                        ItemLivingGradientOutlineVfx>()
                    .Where(value => value != null
                        && value.gameObject.scene
                            == gameObject.scene)
                    .ToArray();
            bool cleanup =
                controller.ActiveEffectCount == 0
                && effects.All(value => !value.IsBound
                    && !value.gameObject.activeSelf);
            Finish(
                cleanup,
                "realPulse=" + realPulseEventId
                + " realAccepted="
                + controller.RealAcceptedTriggerCount
                + " gateAccepted="
                + controller.EventGateAcceptedCount
                + " gateRejected="
                + controller.EventGateRejectedCount
                + " controlledStress=20"
                + " activeCap="
                + controller.PeakActiveEffectCount
                + "/" + controller.ActiveEffectCap
                + " roots="
                + controller.RuntimeEffectRootCount
                + " lifetimeRoots="
                + controller.LifetimeEffectRootCount
                + " materials="
                + controller.SharedMaterialCount
                + " cleanup=" + cleanup
                + " warnings=" + packageWarnings
                + " errors=" + packageErrors
                + " off=" + offPath
                + " persistentA=" + persistentAPath
                + " persistentB=" + persistentBPath
                + " tray=" + trayReturnPath
                + " peak=" + triggerPeakPath
                + " settled=" + settledPath);
        }

        private bool TryCommitOfficialP3PlayFixture()
        {
            QaPlacement[] placements =
            {
                new(
                    "I031",
                    new ItemShapeCell(0, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I009",
                    new ItemShapeCell(1, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I012",
                    new ItemShapeCell(2, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I010",
                    new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation90),
                new(
                    "I008",
                    new ItemShapeCell(0, 2),
                    ItemShapeRotation.Rotation0),
                new(
                    "I011",
                    new ItemShapeCell(1, 3),
                    ItemShapeRotation.Rotation0),
                new(
                    "I007",
                    new ItemShapeCell(2, 4),
                    ItemShapeRotation.Rotation0)
            };
            foreach (QaPlacement placement in placements)
            {
                ItemSystemBattleSandboxBoardOperationResult result =
                    authority.CommitFromTray(
                        placement.itemId,
                        placement.cell,
                        placement.rotation);
                if (!result.Accepted)
                {
                    Finish(
                        false,
                        "official_fixture_rejected="
                        + placement.itemId + ":"
                        + result.DiagnosticCode);
                    return false;
                }
            }
            return true;
        }

        private void CommitI009BackToBoard()
        {
            ItemSystemBattleSandboxBoardOperationResult result =
                authority.CommitFromTray(
                    "I009",
                    new ItemShapeCell(1, 0),
                    ItemShapeRotation.Rotation0);
            if (!result.Accepted)
            {
                Finish(
                    false,
                    "recommit_i009_rejected="
                    + result.DiagnosticCode);
                return;
            }
            SetStage(QaStage.WaitBoardRecommit);
        }

        private bool TryResolveBoardArtwork(out Image image)
        {
            image = null;
            BuildGridInteractionPreviewController grid =
                runtime?.GridController;
            ItemSystemPlacementSnapshot placement =
                grid?.CurrentItemSystemBoardSnapshot
                    ?.placements
                    ?.FirstOrDefault(value => value != null
                        && string.Equals(
                            value.itemId,
                            "I009",
                            StringComparison.Ordinal));
            if (grid == null
                || placement == null
                || !grid.TryResolveBoardItemFeedbackAnchor(
                    "I009",
                    placement.OccupiedCells
                        .Select(value =>
                            new ItemShapeCell(
                                value.x,
                                value.y))
                        .ToArray(),
                    out RectTransform rect,
                    out _,
                    out _,
                    out _)
                || rect == null)
            {
                return false;
            }
            image = rect.GetComponent<Image>();
            return ItemLivingGradientOutlineVfx
                .IsValidAuthoritativeArtwork(image);
        }

        private bool TryResolveTrayArtwork(out Image image)
        {
            BuildItemPreviewCardView card =
                Resources.FindObjectsOfTypeAll<
                        BuildItemPreviewCardView>()
                    .FirstOrDefault(value => value != null
                        && value.gameObject.scene
                            == gameObject.scene
                        && string.Equals(
                            value.ItemId,
                            "I009",
                            StringComparison.Ordinal)
                        && value
                            .HasAuthoritativeArtworkRenderLease
                        && !value.UsesFallbackArtworkRenderLease
                        && value.IsArtworkEffectivelyRendering);
            image = card?.AuthoritativeArtworkImage;
            return ItemLivingGradientOutlineVfx
                .IsValidAuthoritativeArtwork(image);
        }

        private ItemLivingGradientOutlineVfx ResolveI009Effect(
            string surface)
        {
            return Resources.FindObjectsOfTypeAll<
                    ItemLivingGradientOutlineVfx>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene
                        == gameObject.scene
                    && value.IsBound
                    && string.Equals(
                        value.ItemId,
                        "I009",
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.SurfaceId,
                        surface,
                        StringComparison.Ordinal));
        }

        private static bool ValidateLiveEffect(
            ItemLivingGradientOutlineVfx effect,
            out string failure)
        {
            failure = string.Empty;
            if (effect == null)
            {
                failure = "effect_missing";
                return false;
            }
            if (!effect.IsBound
                || !effect.IsPersistent
                || !effect.AddedGraphicsAreRaycastFree
                || !effect.SourceStillExact
                || !ValidateMaterial(
                    effect.PersistentSharedMaterial)
                || !ValidateMaterial(
                    effect.TriggerSharedMaterial)
                || !ValidateMaterial(
                    effect.WholeBodyEmissionSharedMaterial)
                || !ValidateMaterial(
                    effect.WholeBodyInnerGlowSharedMaterial)
                || !ValidateMaterial(
                    effect.WholeBodyOuterGlowSharedMaterial)
                || !ValidateMaterial(
                    effect.EnvironmentSpillSharedMaterial))
            {
                failure =
                    "effect_contract_invalid"
                    + " bound=" + effect.IsBound
                    + " persistent=" + effect.IsPersistent
                    + " raycastFree="
                    + effect.AddedGraphicsAreRaycastFree
                    + " sourceExact="
                    + effect.SourceStillExact;
                return false;
            }
            return true;
        }

        private static bool ValidateMaterial(Material material)
        {
            return material != null
                && material.shader != null
                && material.shader.isSupported
                && string.Equals(
                    material.shader.name,
                    ItemLivingGradientOutlineProfile.ShaderName,
                    StringComparison.Ordinal)
                && !string.Equals(
                    material.shader.name,
                    "Hidden/InternalErrorShader",
                    StringComparison.Ordinal);
        }

        private static bool TryRaycast(
            Image source,
            out GameObject firstHit,
            out bool hitPackageGraphic)
        {
            firstHit = null;
            hitPackageGraphic = false;
            if (source == null
                || source.canvas == null)
            {
                return false;
            }
            Vector3 worldCenter =
                source.rectTransform.TransformPoint(
                    source.rectTransform.rect.center);
            Vector2 screenPoint =
                RectTransformUtility.WorldToScreenPoint(
                    source.canvas.worldCamera,
                    worldCenter);
            Graphic[] hits =
                source.canvas
                    .GetComponentsInChildren<Graphic>(false)
                    .Where(value => value != null
                        && value.raycastTarget
                        && value.gameObject.activeInHierarchy
                        && value.Raycast(
                            screenPoint,
                            source.canvas.worldCamera))
                    .OrderByDescending(value => value.depth)
                    .ToArray();
            firstHit = hits.FirstOrDefault()?.gameObject;
            hitPackageGraphic = hits.Any(value =>
                value.gameObject.name.StartsWith(
                        ItemLivingGradientOutlineVfx
                            .RootNamePrefix,
                        StringComparison.Ordinal)
                    || string.Equals(
                        value.gameObject.name,
                        ItemLivingGradientOutlineVfx
                            .TriggerLayerName,
                        StringComparison.Ordinal));
            // A board artwork may intentionally sit outside the
            // interaction raycast surface; the tray-card cycle below
            // performs the positive hit-owner assertion. For board
            // state, a zero-result raycast is still a valid invariant
            // as long as enabling the VFX does not add a hit.
            return true;
        }

        private void OnLog(
            string condition,
            string stackTrace,
            LogType type)
        {
            string text = condition ?? string.Empty;
            if (text.IndexOf(
                    ItemLivingGradientOutlineProfile.PackageId,
                    StringComparison.Ordinal) < 0
                && text.IndexOf(
                    "ItemLivingGradientOutline",
                    StringComparison.Ordinal) < 0)
            {
                return;
            }
            if (type == LogType.Warning)
            {
                packageWarnings++;
            }
            else if (type == LogType.Error
                || type == LogType.Exception
                || type == LogType.Assert)
            {
                packageErrors++;
            }
        }

        private void Finish(bool passed, string evidence)
        {
            if (finishing)
            {
                return;
            }
            finishing = true;
            Application.logMessageReceived -= OnLog;
            ItemLivingGradientOutlineVfxPrototypeVerifier
                .StoreRenderedDriverResult(
                    passed,
                    evidence ?? string.Empty);
            Debug.Log(
                "["
                + ItemLivingGradientOutlineProfile.PackageId
                + "] RENDERED_DRIVER_FINISH passed="
                + passed + " " + evidence);
            EditorApplication.isPlaying = false;
        }

        private void SetStage(QaStage value)
        {
            stage = value;
            stageStartedAt = Time.unscaledTime;
        }

        private float StageAge =>
            Time.unscaledTime - stageStartedAt;

        private string EvidencePath(string fileName)
        {
            return Path.Combine(captureDirectory, fileName);
        }

        private IEnumerable<string> RequiredEvidencePaths()
        {
            yield return offPath;
            yield return persistentAPath;
            yield return persistentBPath;
            yield return trayReturnPath;
            yield return triggerPeakPath;
            yield return settledPath;
        }

        private static bool EvidenceReady(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && File.Exists(path)
                && new FileInfo(path).Length >= 4096L;
        }

        private static void DeleteEvidence(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
        }
    }
}
