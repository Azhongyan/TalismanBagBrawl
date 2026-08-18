using System;
using UnityEngine;

namespace TalismanBag.Presentation.Items
{
    [Serializable]
    public sealed class ItemRarityContourBloomAppearance
    {
        [SerializeField] private string rarityKey = string.Empty;

        [Header("Seven shared material roles")]
        [SerializeField] private Material contourMaterial;
        [SerializeField] private Material persistentHaloMaterial;
        [SerializeField] private Material wholeBodyEmissionMaterial;
        [SerializeField] private Material innerGlowMaterial;
        [SerializeField] private Material outerHaloMaterial;
        [SerializeField] private Material spillMaterial;
        [SerializeField] private Material triggerMaterial;

        [Header("Persistent living outline")]
        [SerializeField, Range(0f, 1f)] private float persistentAlpha = 0.82f;
        [SerializeField, Range(0f, 1f)] private float persistentHaloAlpha = 0.36f;
        [SerializeField, Range(1f, 1.25f)] private float persistentHaloScale = 1f;
        [SerializeField, Range(0f, 0.25f)] private float breathAmount = 0.08f;
        [SerializeField, Range(0.05f, 1f)] private float breathCyclesPerSecond = 0.3f;

        [Header("Persistent whole-body glow")]
        [SerializeField, Range(0f, 1f)] private float wholeBodyPersistentAlpha = 0.22f;
        [SerializeField, Range(0f, 1f)] private float wholeBodyTriggerAlpha = 0.7f;
        [SerializeField, Range(0f, 1f)] private float innerGlowAlpha = 0.21f;
        [SerializeField, Range(0f, 1f)] private float outerHaloAlpha = 0.11f;
        [SerializeField, Range(0f, 0.3f)] private float wholeBodyBreathAmount = 0.12f;
        [SerializeField, Range(0.45f, 0.72f)] private float wholeBodyBreathCyclesPerSecond = 0.52f;
        [SerializeField, Range(1f, 1.35f)] private float wholeBodyTriggerEndScale = 1.18f;

        [Header("Environment spill")]
        [SerializeField, Range(0f, 1f)] private float environmentSpillPersistentAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float environmentSpillTriggerAlpha = 0.56f;
        [SerializeField, Range(1f, 2.5f)] private float environmentSpillBaseScale = 1.72f;
        [SerializeField, Range(1f, 3f)] private float environmentSpillTriggerScale = 2.08f;

        [Header("Deterministic trigger grammar")]
        [SerializeField, Range(0f, 1f)] private float triggerAlpha = 0.96f;
        [SerializeField, Range(0.05f, 0.2f)] private float triggerAttackSeconds = 0.08f;
        [SerializeField, Range(0.15f, 0.4f)] private float triggerSpreadSeconds = 0.25f;
        [SerializeField, Range(0.35f, 0.8f)] private float triggerSettleSeconds = 0.55f;
        [SerializeField, Range(1f, 1.3f)] private float triggerEndScale = 1.22f;

        public string RarityKey => rarityKey;
        public Material ContourMaterial => contourMaterial;
        public Material PersistentHaloMaterial => persistentHaloMaterial;
        public Material WholeBodyEmissionMaterial => wholeBodyEmissionMaterial;
        public Material InnerGlowMaterial => innerGlowMaterial;
        public Material OuterHaloMaterial => outerHaloMaterial;
        public Material SpillMaterial => spillMaterial;
        public Material TriggerMaterial => triggerMaterial;
        public float PersistentAlpha => persistentAlpha;
        public float PersistentHaloAlpha => persistentHaloAlpha;
        public float PersistentHaloScale => persistentHaloScale;
        public float BreathAmount => breathAmount;
        public float BreathCyclesPerSecond => breathCyclesPerSecond;
        public float WholeBodyPersistentAlpha => wholeBodyPersistentAlpha;
        public float WholeBodyTriggerAlpha => wholeBodyTriggerAlpha;
        public float InnerGlowAlpha => innerGlowAlpha;
        public float OuterHaloAlpha => outerHaloAlpha;
        public float WholeBodyBreathAmount => wholeBodyBreathAmount;
        public float WholeBodyBreathCyclesPerSecond => wholeBodyBreathCyclesPerSecond;
        public float WholeBodyTriggerEndScale => wholeBodyTriggerEndScale;
        public float EnvironmentSpillPersistentAlpha => environmentSpillPersistentAlpha;
        public float EnvironmentSpillTriggerAlpha => environmentSpillTriggerAlpha;
        public float EnvironmentSpillBaseScale => environmentSpillBaseScale;
        public float EnvironmentSpillTriggerScale => environmentSpillTriggerScale;
        public float TriggerAlpha => triggerAlpha;
        public float TriggerAttackSeconds => triggerAttackSeconds;
        public float TriggerSpreadSeconds => triggerSpreadSeconds;
        public float TriggerSettleSeconds => triggerSettleSeconds;
        public float TriggerEndScale => triggerEndScale;

        public bool ValidateAuthoredReferences(string expectedRarityKey)
        {
            return !string.IsNullOrEmpty(expectedRarityKey)
                   && string.Equals(
                       rarityKey,
                       expectedRarityKey,
                       StringComparison.Ordinal)
                   && contourMaterial != null
                   && persistentHaloMaterial != null
                   && wholeBodyEmissionMaterial != null
                   && innerGlowMaterial != null
                   && outerHaloMaterial != null
                   && spillMaterial != null
                   && triggerMaterial != null
                   && persistentAlpha > 0f
                   && persistentHaloAlpha > 0f
                   && persistentHaloScale >= 1f
                   && breathAmount >= 0f
                   && breathCyclesPerSecond > 0f
                   && wholeBodyPersistentAlpha > 0f
                   && wholeBodyTriggerAlpha >= wholeBodyPersistentAlpha
                   && innerGlowAlpha > 0f
                   && outerHaloAlpha > 0f
                   && wholeBodyBreathAmount >= 0f
                   && wholeBodyBreathCyclesPerSecond > 0f
                   && wholeBodyTriggerEndScale >= 1f
                   && environmentSpillPersistentAlpha > 0f
                   && environmentSpillTriggerAlpha
                   >= environmentSpillPersistentAlpha
                   && environmentSpillBaseScale >= 1f
                   && environmentSpillTriggerScale
                   >= environmentSpillBaseScale
                   && triggerAlpha > 0f
                   && triggerAttackSeconds > 0f
                   && triggerSpreadSeconds >= triggerAttackSeconds
                   && triggerSettleSeconds >= triggerSpreadSeconds
                   && triggerEndScale >= 1f;
        }

#if UNITY_EDITOR
        internal static ItemRarityContourBloomAppearance CloneForEditor(
            ItemRarityContourBloomAppearance source,
            string configuredRarityKey)
        {
            if (source == null)
            {
                return null;
            }

            return new ItemRarityContourBloomAppearance
            {
                rarityKey = configuredRarityKey ?? string.Empty,
                contourMaterial = source.contourMaterial,
                persistentHaloMaterial = source.persistentHaloMaterial,
                wholeBodyEmissionMaterial = source.wholeBodyEmissionMaterial,
                innerGlowMaterial = source.innerGlowMaterial,
                outerHaloMaterial = source.outerHaloMaterial,
                spillMaterial = source.spillMaterial,
                triggerMaterial = source.triggerMaterial,
                persistentAlpha = source.persistentAlpha,
                persistentHaloAlpha = source.persistentHaloAlpha,
                persistentHaloScale = source.persistentHaloScale,
                breathAmount = source.breathAmount,
                breathCyclesPerSecond = source.breathCyclesPerSecond,
                wholeBodyPersistentAlpha = source.wholeBodyPersistentAlpha,
                wholeBodyTriggerAlpha = source.wholeBodyTriggerAlpha,
                innerGlowAlpha = source.innerGlowAlpha,
                outerHaloAlpha = source.outerHaloAlpha,
                wholeBodyBreathAmount = source.wholeBodyBreathAmount,
                wholeBodyBreathCyclesPerSecond =
                    source.wholeBodyBreathCyclesPerSecond,
                wholeBodyTriggerEndScale = source.wholeBodyTriggerEndScale,
                environmentSpillPersistentAlpha =
                    source.environmentSpillPersistentAlpha,
                environmentSpillTriggerAlpha =
                    source.environmentSpillTriggerAlpha,
                environmentSpillBaseScale =
                    source.environmentSpillBaseScale,
                environmentSpillTriggerScale =
                    source.environmentSpillTriggerScale,
                triggerAlpha = source.triggerAlpha,
                triggerAttackSeconds = source.triggerAttackSeconds,
                triggerSpreadSeconds = source.triggerSpreadSeconds,
                triggerSettleSeconds = source.triggerSettleSeconds,
                triggerEndScale = source.triggerEndScale
            };
        }
#endif
    }

    [CreateAssetMenu(
        fileName = "ItemRarityContourBloomProfile",
        menuName = "TalismanBag/Presentation/Item Rarity Contour Bloom Profile")]
    public sealed class ItemRarityContourBloomProfile : ScriptableObject
    {
        public const string ProfileIdValue =
            "item_living_gradient_outline_exact_v2";
        public const string WhiteKey = "white";
        public const string GreenKey = "green";
        public const string BlueKey = "blue";
        public const string PurpleKey = "purple";
        public const string OrangeKey = "orange";
        public const string SpecialSourceKey = "special-source";

        [SerializeField] private string profileId = ProfileIdValue;
        [SerializeField] private ItemRarityContourBloomAppearance white = new();
        [SerializeField] private ItemRarityContourBloomAppearance green = new();
        [SerializeField] private ItemRarityContourBloomAppearance blue = new();
        [SerializeField] private ItemRarityContourBloomAppearance purple = new();
        [SerializeField] private ItemRarityContourBloomAppearance orange = new();
        [SerializeField] private ItemRarityContourBloomAppearance
            specialSource = new();

        public string ProfileId => profileId;

        public bool TryResolve(
            string authoritativeRarityKey,
            out ItemRarityContourBloomAppearance appearance)
        {
            appearance = null;
            string expectedRarityKey;
            if (string.IsNullOrWhiteSpace(authoritativeRarityKey))
            {
                return false;
            }

            switch (authoritativeRarityKey.Trim().ToLowerInvariant())
            {
                case WhiteKey:
                    appearance = white;
                    expectedRarityKey = WhiteKey;
                    break;
                case GreenKey:
                    appearance = green;
                    expectedRarityKey = GreenKey;
                    break;
                case BlueKey:
                    appearance = blue;
                    expectedRarityKey = BlueKey;
                    break;
                case PurpleKey:
                    appearance = purple;
                    expectedRarityKey = PurpleKey;
                    break;
                case OrangeKey:
                    appearance = orange;
                    expectedRarityKey = OrangeKey;
                    break;
                case SpecialSourceKey:
                    appearance = specialSource;
                    expectedRarityKey = SpecialSourceKey;
                    break;
                default:
                    return false;
            }

            return appearance != null
                   && appearance.ValidateAuthoredReferences(
                       expectedRarityKey);
        }

        public bool ValidateAuthoredReferences()
        {
            return string.Equals(
                       profileId,
                       ProfileIdValue,
                       StringComparison.Ordinal)
                   && white != null
                   && white.ValidateAuthoredReferences(WhiteKey)
                   && green != null
                   && green.ValidateAuthoredReferences(GreenKey)
                   && blue != null
                   && blue.ValidateAuthoredReferences(BlueKey)
                   && purple != null
                   && purple.ValidateAuthoredReferences(PurpleKey)
                   && orange != null
                   && orange.ValidateAuthoredReferences(OrangeKey)
                   && specialSource != null
                   && specialSource.ValidateAuthoredReferences(
                       SpecialSourceKey);
        }

#if UNITY_EDITOR
        public void AssignSpecialSourceForEditor(
            ItemRarityContourBloomAppearance source)
        {
            specialSource = ItemRarityContourBloomAppearance.CloneForEditor(
                source,
                SpecialSourceKey);
        }
#endif
    }
}
