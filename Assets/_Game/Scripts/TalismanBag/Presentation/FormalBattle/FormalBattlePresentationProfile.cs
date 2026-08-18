using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TalismanBag.Presentation.FormalBattle
{
    [Serializable]
    public sealed class FormalBattleAnimationClip
    {
        [SerializeField] private string clipId = string.Empty;
        [SerializeField] private Texture2D[] frames = Array.Empty<Texture2D>();
        [SerializeField, Min(1)] private int frameDurationMilliseconds = 100;
        [SerializeField] private bool loops;
        [SerializeField] private RectInt referencePixelBounds;
        [SerializeField] private RectInt unionPixelBounds;

        public string ClipId => clipId ?? string.Empty;
        public Texture2D[] Frames => frames ?? Array.Empty<Texture2D>();
        public int FrameDurationMilliseconds =>
            Mathf.Max(1, frameDurationMilliseconds);
        public bool Loops => loops;
        public RectInt ReferencePixelBounds => referencePixelBounds;
        public RectInt UnionPixelBounds => unionPixelBounds;
        public float DurationSeconds =>
            Frames.Length * FrameDurationMilliseconds / 1000f;

#if UNITY_EDITOR
        public void AssignForEditor(
            string configuredClipId,
            Texture2D[] configuredFrames,
            int configuredFrameDurationMilliseconds,
            bool configuredLoops,
            RectInt configuredReferencePixelBounds,
            RectInt configuredUnionPixelBounds)
        {
            clipId = configuredClipId ?? string.Empty;
            frames = configuredFrames ?? Array.Empty<Texture2D>();
            frameDurationMilliseconds = Mathf.Max(
                1,
                configuredFrameDurationMilliseconds);
            loops = configuredLoops;
            referencePixelBounds = configuredReferencePixelBounds;
            unionPixelBounds = configuredUnionPixelBounds;
        }
#endif
    }

    [Serializable]
    public sealed class FormalBattleEnemyVisualProfile
    {
        [SerializeField] private string contentId = string.Empty;
        [SerializeField] private string runtimeProfileId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private FormalBattleAnimationClip idle;
        [SerializeField] private FormalBattleAnimationClip attack;
        [SerializeField] private FormalBattleAnimationClip hit;
        [SerializeField] private FormalBattleAnimationClip death;

        public string ContentId => contentId ?? string.Empty;
        public string RuntimeProfileId => runtimeProfileId ?? string.Empty;
        public string DisplayName => displayName ?? string.Empty;
        public FormalBattleAnimationClip Idle => idle;
        public FormalBattleAnimationClip Attack => attack;
        public FormalBattleAnimationClip Hit => hit;
        public FormalBattleAnimationClip Death => death;

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(ContentId)
                   && !string.IsNullOrWhiteSpace(RuntimeProfileId)
                   && !string.IsNullOrWhiteSpace(DisplayName)
                   && ValidateClip(idle)
                   && ValidateClip(attack)
                   && ValidateClip(hit)
                   && ValidateClip(death);
        }

        private static bool ValidateClip(FormalBattleAnimationClip clip)
        {
            return clip != null
                   && !string.IsNullOrWhiteSpace(clip.ClipId)
                   && clip.Frames.Length > 0
                   && clip.Frames.All(value => value != null);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            string configuredContentId,
            string configuredRuntimeProfileId,
            string configuredDisplayName,
            FormalBattleAnimationClip configuredIdle,
            FormalBattleAnimationClip configuredAttack,
            FormalBattleAnimationClip configuredHit,
            FormalBattleAnimationClip configuredDeath)
        {
            contentId = configuredContentId ?? string.Empty;
            runtimeProfileId = configuredRuntimeProfileId ?? string.Empty;
            displayName = configuredDisplayName ?? string.Empty;
            idle = configuredIdle;
            attack = configuredAttack;
            hit = configuredHit;
            death = configuredDeath;
        }
#endif
    }

    [Serializable]
    public sealed class FormalBattleCausalItemStyle
    {
        [FormerlySerializedAs("authoritativeBaseItemId")]
        [SerializeField] private string grammarKey = string.Empty;
        [SerializeField] private string presentationStyleKey = string.Empty;
        [SerializeField] private string effectFamilyKey = string.Empty;
        [SerializeField] private string cueIdentity = string.Empty;
        [SerializeField] private string cueKind = string.Empty;
        [SerializeField] private string effectVariantId = string.Empty;
        [FormerlySerializedAs("sourceArtwork")]
        [SerializeField, HideInInspector] private Sprite retiredSourceArtwork;
        [SerializeField] private Color sourcePulseColor =
            new Color(1f, 0.64f, 0.18f, 0.9f);
        [SerializeField] private Color proxyHaloColor =
            new Color(1f, 0.48f, 0.12f, 0.72f);
        [SerializeField] private Color ribbonCoreColor =
            new Color(1f, 0.78f, 0.32f, 0.92f);
        [SerializeField] private Color ribbonGlowColor =
            new Color(1f, 0.32f, 0.08f, 0.28f);
        [SerializeField, Min(2)] private int ribbonSegmentCount = 6;
        [SerializeField, Min(0f)] private float ribbonWaveAmplitude = 7f;
        [SerializeField, Min(0.25f)] private float ribbonWaveCycles = 1f;
        [SerializeField, Min(1f)] private float ribbonCoreWidth = 7f;
        [SerializeField, Min(1f)] private float ribbonGlowWidth = 22f;
        [SerializeField, Min(0.1f)] private float sourcePulseCadence = 1f;

        [NonSerialized] private Sprite resolvedSourceArtwork;

        public string GrammarKey => Normalize(grammarKey);
        public string PresentationStyleKey => Normalize(
            presentationStyleKey);
        public string EffectFamilyKey => Normalize(effectFamilyKey);
        public string CueIdentity => Normalize(cueIdentity);
        public string CueKind => Normalize(cueKind);
        public string EffectVariantId => Normalize(effectVariantId);
        public Sprite SourceArtwork => resolvedSourceArtwork;
        public Color SourcePulseColor => sourcePulseColor;
        public Color ProxyHaloColor => proxyHaloColor;
        public Color RibbonCoreColor => ribbonCoreColor;
        public Color RibbonGlowColor => ribbonGlowColor;
        public int RibbonSegmentCount => Mathf.Max(2, ribbonSegmentCount);
        public float RibbonWaveAmplitude =>
            Mathf.Max(0f, ribbonWaveAmplitude);
        public float RibbonWaveCycles => Mathf.Max(0.25f, ribbonWaveCycles);
        public float RibbonCoreWidth => Mathf.Max(1f, ribbonCoreWidth);
        public float RibbonGlowWidth => Mathf.Max(1f, ribbonGlowWidth);
        public float SourcePulseCadence => Mathf.Max(0.1f, sourcePulseCadence);

        public bool Validate()
        {
            return GrammarKey.Length > 0
                   && EffectFamilyKey.Length > 0
                   && sourcePulseColor.a > 0f
                   && proxyHaloColor.a > 0f
                   && ribbonCoreColor.a > 0f
                   && ribbonGlowColor.a > 0f
                   && RibbonSegmentCount >= 2
                   && RibbonCoreWidth <= RibbonGlowWidth;
        }

        public bool Matches(
            string authoritativePresentationStyleKey,
            string authoritativeEffectFamilyKey,
            string authoritativeCueIdentity,
            string authoritativeCueKind,
            string authoritativeEffectVariantId)
        {
            string exactPresentationStyleKey = Normalize(
                authoritativePresentationStyleKey);
            string exactEffectFamilyKey = Normalize(
                authoritativeEffectFamilyKey);
            string exactCueIdentity = Normalize(authoritativeCueIdentity);
            string exactCueKind = Normalize(authoritativeCueKind);
            string exactEffectVariantId = Normalize(
                authoritativeEffectVariantId);
            return Validate()
                   && exactPresentationStyleKey.Length > 0
                   && exactEffectFamilyKey.Length > 0
                   && exactCueIdentity.Length > 0
                   && exactCueKind.Length > 0
                   && string.Equals(
                       EffectFamilyKey,
                       exactEffectFamilyKey,
                       StringComparison.Ordinal)
                   && SelectorMatches(
                       PresentationStyleKey,
                       exactPresentationStyleKey)
                   && SelectorMatches(CueIdentity, exactCueIdentity)
                   && SelectorMatches(CueKind, exactCueKind)
                   && SelectorMatches(
                       EffectVariantId,
                       exactEffectVariantId);
        }

        public int MatchSpecificity
        {
            get
            {
                int score = 0;
                if (PresentationStyleKey.Length > 0) score += 8;
                if (CueIdentity.Length > 0) score += 4;
                if (CueKind.Length > 0) score += 2;
                if (EffectVariantId.Length > 0) score += 1;
                return score;
            }
        }

        internal bool BindResolvedSourceArtwork(Sprite artwork)
        {
            resolvedSourceArtwork = artwork;
            return resolvedSourceArtwork != null;
        }

        internal void ClearResolvedSourceArtwork()
        {
            resolvedSourceArtwork = null;
        }

        private static bool SelectorMatches(
            string selector,
            string authoritativeValue)
        {
            return selector.Length == 0
                   || string.Equals(
                       selector,
                       authoritativeValue,
                       StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            string configuredGrammarKey,
            string configuredPresentationStyleKey,
            string configuredEffectFamilyKey,
            string configuredCueIdentity,
            string configuredCueKind,
            string configuredEffectVariantId,
            Color configuredSourcePulseColor,
            Color configuredProxyHaloColor,
            Color configuredRibbonCoreColor,
            Color configuredRibbonGlowColor,
            int configuredRibbonSegmentCount,
            float configuredRibbonWaveAmplitude,
            float configuredRibbonWaveCycles,
            float configuredRibbonCoreWidth,
            float configuredRibbonGlowWidth,
            float configuredSourcePulseCadence)
        {
            grammarKey = Normalize(configuredGrammarKey);
            presentationStyleKey = Normalize(
                configuredPresentationStyleKey);
            effectFamilyKey = Normalize(configuredEffectFamilyKey);
            cueIdentity = Normalize(configuredCueIdentity);
            cueKind = Normalize(configuredCueKind);
            effectVariantId = Normalize(configuredEffectVariantId);
            retiredSourceArtwork = null;
            resolvedSourceArtwork = null;
            sourcePulseColor = configuredSourcePulseColor;
            proxyHaloColor = configuredProxyHaloColor;
            ribbonCoreColor = configuredRibbonCoreColor;
            ribbonGlowColor = configuredRibbonGlowColor;
            ribbonSegmentCount = Mathf.Max(2, configuredRibbonSegmentCount);
            ribbonWaveAmplitude = Mathf.Max(
                0f,
                configuredRibbonWaveAmplitude);
            ribbonWaveCycles = Mathf.Max(0.25f, configuredRibbonWaveCycles);
            ribbonCoreWidth = Mathf.Max(1f, configuredRibbonCoreWidth);
            ribbonGlowWidth = Mathf.Max(
                ribbonCoreWidth,
                configuredRibbonGlowWidth);
            sourcePulseCadence = Mathf.Max(
                0.1f,
                configuredSourcePulseCadence);
        }

        public bool ValidateVisualGrammarForEditor()
        {
            return sourcePulseColor.a > 0f
                   && proxyHaloColor.a > 0f
                   && ribbonCoreColor.a > 0f
                   && ribbonGlowColor.a > 0f
                   && RibbonSegmentCount >= 2
                   && RibbonCoreWidth <= RibbonGlowWidth;
        }
#endif
    }

    public enum FormalBattleStatusPolarity
    {
        Positive = 0,
        Negative = 1
    }

    [Serializable]
    public sealed class FormalBattleStatusVisualStyle
    {
        [SerializeField] private string statusKey = string.Empty;
        [SerializeField] private string statusFamilyKey = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string glyph = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private FormalBattleStatusPolarity polarity =
            FormalBattleStatusPolarity.Negative;
        [SerializeField] private Color backgroundColor =
            new Color(0.34f, 0.10f, 0.08f, 0.94f);
        [SerializeField] private Color foregroundColor =
            new Color(1f, 0.87f, 0.58f, 1f);

        public string StatusKey => Normalize(statusKey);
        public string StatusFamilyKey => Normalize(statusFamilyKey);
        public string DisplayName => Normalize(displayName);
        public string Glyph => Normalize(glyph);
        public Sprite Icon => icon;
        public FormalBattleStatusPolarity Polarity => polarity;
        public Color BackgroundColor => backgroundColor;
        public Color ForegroundColor => foregroundColor;

        public bool Validate()
        {
            return StatusKey.Length > 0
                   && StatusFamilyKey.Length > 0
                   && DisplayName.Length > 0
                   && Icon != null
                   && backgroundColor.a > 0f
                   && foregroundColor.a > 0f;
        }

        public bool Matches(
            string authoritativeStatusKey,
            string authoritativeStatusFamilyKey)
        {
            return Validate()
                   && string.Equals(
                       StatusKey,
                       Normalize(authoritativeStatusKey),
                       StringComparison.Ordinal)
                   && string.Equals(
                       StatusFamilyKey,
                       Normalize(authoritativeStatusFamilyKey),
                       StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            string configuredStatusKey,
            string configuredStatusFamilyKey,
            string configuredDisplayName,
            string configuredGlyph,
            Sprite configuredIcon,
            FormalBattleStatusPolarity configuredPolarity,
            Color configuredBackgroundColor,
            Color configuredForegroundColor)
        {
            statusKey = Normalize(configuredStatusKey);
            statusFamilyKey = Normalize(configuredStatusFamilyKey);
            displayName = Normalize(configuredDisplayName);
            glyph = Normalize(configuredGlyph);
            icon = configuredIcon;
            polarity = configuredPolarity;
            backgroundColor = configuredBackgroundColor;
            foregroundColor = configuredForegroundColor;
        }
#endif
    }

    public static class FormalBattleDamageFloatStyleKeys
    {
        public const string Damage = "DAMAGE";
        public const string Heal = "HEAL";
        public const string Guard = "GUARD";
        public const string Shell = "SHELL";
        public const string Nian = "NIAN";
        public const string Cleanse = "CLEANSE";
        public const string Control = "CONTROL";

        public static readonly string[] All =
        {
            Damage,
            Heal,
            Guard,
            Shell,
            Nian,
            Cleanse,
            Control
        };
    }

    [Serializable]
    public sealed class FormalBattleDamageFloatVisualStyle
    {
        [SerializeField] private string styleKey = string.Empty;
        [SerializeField] private Color topColor = Color.white;
        [SerializeField] private Color middleColor = Color.white;
        [SerializeField] private Color bottomColor = Color.white;

        public string StyleKey => Normalize(styleKey);
        public Color TopColor => topColor;
        public Color MiddleColor => middleColor;
        public Color BottomColor => bottomColor;
        public VertexGradient Gradient => new VertexGradient(
            topColor,
            topColor,
            bottomColor,
            middleColor);

        public bool Validate()
        {
            return StyleKey.Length > 0
                   && topColor.a > 0f
                   && middleColor.a > 0f
                   && bottomColor.a > 0f;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            string configuredStyleKey,
            Color configuredTopColor,
            Color configuredMiddleColor,
            Color configuredBottomColor)
        {
            styleKey = Normalize(configuredStyleKey);
            topColor = configuredTopColor;
            middleColor = configuredMiddleColor;
            bottomColor = configuredBottomColor;
        }
#endif
    }

    [CreateAssetMenu(
        fileName = "FormalBattlePresentationProfile",
        menuName = "TalismanBag/V0.4/Formal Battle Presentation Profile")]
    public sealed class FormalBattlePresentationProfile : ScriptableObject
    {
        public const string NianResourceGainGrammarKey =
            "NIAN_RESOURCE_GAIN";

        private static readonly string[] ExpectedEnemyContentIds =
        {
            "bone_aspect_enemy_c1_01_shattered_host",
            "bone_aspect_enemy_c1_02_porcelain_hound",
            "bone_aspect_enemy_c1_03_bone_swap_remnant"
        };

        private static readonly string[] ExpectedEnemyRuntimeProfileIds =
        {
            "bone_aspect.runtime.c1.shattered_host.v1",
            "bone_aspect.runtime.c1.porcelain_hound.v1",
            "bone_aspect.runtime.c1.bone_swap_remnant.operator.v1"
        };

        private static readonly string[] ExpectedEnemyDisplayNames =
        {
            "\u788E\u5951\u5BBF\u4E3B",
            "\u74F7\u9AA8\u730E\u72AC",
            "\u6362\u9AA8\u6B8B\u76F8"
        };

        [Header("Accepted authored references")]
        [SerializeField] private Sprite playerAvatarSprite;
        [SerializeField] private Sprite hpFrameSprite;
        [SerializeField] private TMP_FontAsset damageFont;
        [SerializeField] private Texture2D restrainedVfxTexture;
        [SerializeField] private AudioClip sourcePulseClip;
        [SerializeField] private AudioClip enemyAttackClip;
        [SerializeField] private AudioClip impactClip;
        [SerializeField] private FormalBattleEnemyVisualProfile[] enemyProfiles =
            Array.Empty<FormalBattleEnemyVisualProfile>();
        [SerializeField] private FormalBattleCausalItemStyle[] causalItemStyles =
            Array.Empty<FormalBattleCausalItemStyle>();
        [SerializeField] private FormalBattleStatusVisualStyle[] statusStyles =
            Array.Empty<FormalBattleStatusVisualStyle>();
        [SerializeField] private FormalBattleDamageFloatVisualStyle[]
            damageFloatStyles =
                Array.Empty<FormalBattleDamageFloatVisualStyle>();

        [Header("Accepted visual grammar")]
        [SerializeField] private Color hpDamageTop =
            new Color(1f, 0.84f, 0.36f, 1f);
        [SerializeField] private Color hpDamageMiddle =
            new Color(1f, 0.36f, 0.08f, 1f);
        [SerializeField] private Color hpDamageBottom =
            new Color(0.72f, 0.07f, 0.06f, 1f);
        [SerializeField] private Color targetOutlineColor =
            new Color(1f, 0.82f, 0.28f, 0.92f);
        [SerializeField] private Color hpFillColor =
            new Color(0.73f, 0.12f, 0.09f, 1f);
        [SerializeField] private Color sourcePulseColor =
            new Color(1f, 0.64f, 0.18f, 0.88f);
        [SerializeField, Min(0.1f)] private float causalWindupDuration = 0.42f;
        [SerializeField, Min(0.1f)] private float causalSourceHoldDuration = 0.72f;
        [SerializeField, Min(0.1f)] private float causalRibbonDuration = 0.28f;

        public Sprite PlayerAvatarSprite => playerAvatarSprite;
        public Sprite HpFrameSprite => hpFrameSprite;
        public TMP_FontAsset DamageFont => damageFont;
        public Texture2D RestrainedVfxTexture => restrainedVfxTexture;
        public AudioClip SourcePulseClip => sourcePulseClip;
        public AudioClip EnemyAttackClip => enemyAttackClip;
        public AudioClip ImpactClip => impactClip;
        public Color HpDamageTop => hpDamageTop;
        public Color HpDamageMiddle => hpDamageMiddle;
        public Color HpDamageBottom => hpDamageBottom;
        public Color TargetOutlineColor => targetOutlineColor;
        public Color HpFillColor => hpFillColor;
        public Color SourcePulseColor => sourcePulseColor;
        public float CausalWindupDuration =>
            Mathf.Max(0.1f, causalWindupDuration);
        public float CausalSourceHoldDuration =>
            Mathf.Max(0.1f, causalSourceHoldDuration);
        public float CausalRibbonDuration =>
            Mathf.Max(0.1f, causalRibbonDuration);

        public bool TryGetDamageFloatStyle(
            string styleKey,
            out FormalBattleDamageFloatVisualStyle style)
        {
            style = null;
            string normalizedKey = string.IsNullOrWhiteSpace(styleKey)
                ? string.Empty
                : styleKey.Trim();
            FormalBattleDamageFloatVisualStyle[] matches =
                (damageFloatStyles
                    ?? Array.Empty<FormalBattleDamageFloatVisualStyle>())
                .Where(value => value != null
                    && value.Validate()
                    && string.Equals(
                        value.StyleKey,
                        normalizedKey,
                        StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                return false;
            }

            style = matches[0];
            return true;
        }

        public bool TryGetEnemyProfile(
            string contentId,
            string runtimeProfileId,
            out FormalBattleEnemyVisualProfile profile)
        {
            profile = (enemyProfiles ?? Array.Empty<
                    FormalBattleEnemyVisualProfile>())
                .SingleOrDefault(value => value != null
                    && string.Equals(
                        value.ContentId,
                        contentId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.RuntimeProfileId,
                        runtimeProfileId,
                        StringComparison.Ordinal));
            return profile != null && profile.Validate();
        }

        public bool TryGetCausalItemStyle(
            string grammarKey,
            out FormalBattleCausalItemStyle style)
        {
            style = (causalItemStyles
                     ?? Array.Empty<FormalBattleCausalItemStyle>())
                .SingleOrDefault(value => value != null
                    && string.Equals(
                        value.GrammarKey,
                        grammarKey,
                        StringComparison.Ordinal));
            return style != null && style.Validate();
        }

        public bool TryGetStatusVisualStyle(
            string statusKey,
            string statusFamilyKey,
            out FormalBattleStatusVisualStyle style)
        {
            style = (statusStyles
                     ?? Array.Empty<FormalBattleStatusVisualStyle>())
                .SingleOrDefault(value => value != null
                    && value.Matches(statusKey, statusFamilyKey));
            return style != null;
        }

        public bool TryResolveCausalGrammar(
            string authoritativePresentationStyleKey,
            string authoritativeEffectFamilyKey,
            string authoritativeCueIdentity,
            string authoritativeCueKind,
            string authoritativeEffectVariantId,
            out string resolvedGrammarKey,
            out FormalBattleCausalItemStyle style)
        {
            resolvedGrammarKey = string.Empty;
            style = null;
            FormalBattleCausalItemStyle[] matches = (causalItemStyles
                    ?? Array.Empty<FormalBattleCausalItemStyle>())
                .Where(value => value != null && value.Matches(
                    authoritativePresentationStyleKey,
                    authoritativeEffectFamilyKey,
                    authoritativeCueIdentity,
                    authoritativeCueKind,
                    authoritativeEffectVariantId))
                .OrderByDescending(value => value.MatchSpecificity)
                .ToArray();
            if (matches.Length == 0)
            {
                return false;
            }

            int bestSpecificity = matches[0].MatchSpecificity;
            if (matches.Count(value => value.MatchSpecificity
                                       == bestSpecificity) != 1)
            {
                return false;
            }

            style = matches[0];
            resolvedGrammarKey = style.GrammarKey;
            return resolvedGrammarKey.Length > 0;
        }

        internal void ClearTransientCausalArtwork()
        {
            foreach (FormalBattleCausalItemStyle style in causalItemStyles
                         ?? Array.Empty<FormalBattleCausalItemStyle>())
            {
                style?.ClearResolvedSourceArtwork();
            }
        }

        public bool ValidateAuthoredReferences()
        {
            FormalBattleEnemyVisualProfile[] rows =
                enemyProfiles ?? Array.Empty<FormalBattleEnemyVisualProfile>();
            FormalBattleCausalItemStyle[] causalRows =
                causalItemStyles
                ?? Array.Empty<FormalBattleCausalItemStyle>();
            FormalBattleStatusVisualStyle[] statusRows =
                statusStyles
                ?? Array.Empty<FormalBattleStatusVisualStyle>();
            FormalBattleDamageFloatVisualStyle[] floatRows =
                damageFloatStyles
                ?? Array.Empty<FormalBattleDamageFloatVisualStyle>();
            return playerAvatarSprite != null
                   && hpFrameSprite != null
                   && damageFont != null
                   && rows.Length == ExpectedEnemyContentIds.Length
                   && rows.All(value => value != null && value.Validate())
                   && rows.Select(value => value.ContentId).SequenceEqual(
                       ExpectedEnemyContentIds,
                       StringComparer.Ordinal)
                   && rows.Select(value => value.RuntimeProfileId).SequenceEqual(
                       ExpectedEnemyRuntimeProfileIds,
                       StringComparer.Ordinal)
                   && rows.Select(value => value.DisplayName).SequenceEqual(
                       ExpectedEnemyDisplayNames,
                       StringComparer.Ordinal)
                   && rows.Select(value => value.ContentId)
                       .Distinct(StringComparer.Ordinal).Count() == rows.Length
                   && rows.Select(value => value.RuntimeProfileId)
                       .Distinct(StringComparer.Ordinal).Count() == rows.Length
                   && causalRows.Length > 0
                   && causalRows.All(value => value != null && value.Validate())
                   && causalRows.Select(value => value.GrammarKey)
                       .Distinct(StringComparer.Ordinal).Count()
                        == causalRows.Length
                   && causalRows.Select(value => string.Join("|", new[]
                       {
                           value.PresentationStyleKey,
                           value.EffectFamilyKey,
                           value.CueIdentity,
                           value.CueKind,
                           value.EffectVariantId
                       }))
                       .Distinct(StringComparer.Ordinal).Count()
                        == causalRows.Length
                   && statusRows.Length > 0
                   && statusRows.All(value => value != null
                       && value.Validate())
                   && statusRows.Select(value => value.StatusKey + "|"
                           + value.StatusFamilyKey)
                       .Distinct(StringComparer.Ordinal).Count()
                        == statusRows.Length
                   && floatRows.Length
                        == FormalBattleDamageFloatStyleKeys.All.Length
                   && floatRows.All(value => value != null
                       && value.Validate())
                   && floatRows.Select(value => value.StyleKey)
                       .SequenceEqual(
                           FormalBattleDamageFloatStyleKeys.All,
                           StringComparer.Ordinal)
                   && CausalWindupDuration <= CausalSourceHoldDuration
                   && CausalRibbonDuration <= CausalSourceHoldDuration;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            Sprite configuredPlayerAvatarSprite,
            Sprite configuredHpFrameSprite,
            TMP_FontAsset configuredDamageFont,
            Texture2D configuredRestrainedVfxTexture,
            AudioClip configuredSourcePulseClip,
            AudioClip configuredEnemyAttackClip,
            AudioClip configuredImpactClip,
            FormalBattleEnemyVisualProfile[] configuredEnemyProfiles)
        {
            playerAvatarSprite = configuredPlayerAvatarSprite;
            hpFrameSprite = configuredHpFrameSprite;
            damageFont = configuredDamageFont;
            restrainedVfxTexture = configuredRestrainedVfxTexture;
            sourcePulseClip = configuredSourcePulseClip;
            enemyAttackClip = configuredEnemyAttackClip;
            impactClip = configuredImpactClip;
            enemyProfiles = configuredEnemyProfiles
                ?? Array.Empty<FormalBattleEnemyVisualProfile>();
        }


        public void AssignCausalVisualsForEditor(
            FormalBattleCausalItemStyle[] configuredStyles,
            float configuredWindupDuration,
            float configuredSourceHoldDuration,
            float configuredRibbonDuration)
        {
            causalItemStyles = configuredStyles
                ?? Array.Empty<FormalBattleCausalItemStyle>();
            causalWindupDuration = Mathf.Max(
                0.1f,
                configuredWindupDuration);
            causalSourceHoldDuration = Mathf.Max(
                causalWindupDuration,
                configuredSourceHoldDuration);
            causalRibbonDuration = Mathf.Clamp(
                configuredRibbonDuration,
                0.1f,
                causalSourceHoldDuration);
        }

        public FormalBattleCausalItemStyle[] GetCausalStylesForEditor()
        {
            return (causalItemStyles
                    ?? Array.Empty<FormalBattleCausalItemStyle>())
                .ToArray();
        }

        public void AssignStatusVisualsForEditor(
            FormalBattleStatusVisualStyle[] configuredStyles)
        {
            statusStyles = configuredStyles
                ?? Array.Empty<FormalBattleStatusVisualStyle>();
        }

        public FormalBattleStatusVisualStyle[] GetStatusStylesForEditor()
        {
            return (statusStyles
                    ?? Array.Empty<FormalBattleStatusVisualStyle>())
                .ToArray();
        }

        public void AssignDamageFloatVisualsForEditor(
            FormalBattleDamageFloatVisualStyle[] configuredStyles)
        {
            damageFloatStyles = configuredStyles
                ?? Array.Empty<FormalBattleDamageFloatVisualStyle>();
        }

        public FormalBattleDamageFloatVisualStyle[]
            GetDamageFloatStylesForEditor()
        {
            return (damageFloatStyles
                    ?? Array.Empty<FormalBattleDamageFloatVisualStyle>())
                .ToArray();
        }
#endif
    }
}
