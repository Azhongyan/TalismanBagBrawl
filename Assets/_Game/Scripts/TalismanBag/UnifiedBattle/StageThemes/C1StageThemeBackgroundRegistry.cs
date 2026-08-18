using System;
using TalismanBag.Presentation.StageThemes;
using UnityEngine;

namespace TalismanBag.UnifiedBattle.StageThemes
{
    [CreateAssetMenu(
        fileName = "C1StageThemeBackgroundRegistry",
        menuName = "TalismanBag/V0.4/Unified Battle/C1 Stage Theme Background Registry")]
    public sealed class C1StageThemeBackgroundRegistry : ScriptableObject
    {
        public const string InteriorProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1";
        public const string ExteriorProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1.exterior";
        public const string NightAnimatedProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1.night_animated";

        [SerializeField] private C1StageThemeBackgroundProfile interiorProfile;
        [SerializeField] private C1StageThemeBackgroundProfile exteriorProfile;
        [SerializeField] private C1StageThemeBackgroundProfile nightAnimatedProfile;

        public C1StageThemeBackgroundProfile InteriorProfile => interiorProfile;
        public C1StageThemeBackgroundProfile ExteriorProfile => exteriorProfile;
        public C1StageThemeBackgroundProfile NightAnimatedProfile =>
            nightAnimatedProfile;

        public bool TryValidate(out string diagnostic)
        {
            if (interiorProfile == null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_REFERENCE_MISSING slot=interior";
                return false;
            }

            if (exteriorProfile == null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_REFERENCE_MISSING slot=exterior";
                return false;
            }

            if (nightAnimatedProfile == null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_REFERENCE_MISSING slot=night_animated";
                return false;
            }

            if (ReferenceEquals(interiorProfile, exteriorProfile))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_DUPLICATE_REFERENCE slots=interior,exterior";
                return false;
            }

            if (ReferenceEquals(interiorProfile, nightAnimatedProfile))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_DUPLICATE_REFERENCE slots=interior,night_animated";
                return false;
            }

            if (ReferenceEquals(exteriorProfile, nightAnimatedProfile))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_DUPLICATE_REFERENCE slots=exterior,night_animated";
                return false;
            }

            if (!ValidateExactProfile(
                    interiorProfile,
                    "interior",
                    InteriorProfileId,
                    out diagnostic)
                || !ValidateExactProfile(
                    exteriorProfile,
                    "exterior",
                    ExteriorProfileId,
                    out diagnostic)
                || !ValidateExactProfile(
                    nightAnimatedProfile,
                    "night_animated",
                    NightAnimatedProfileId,
                    out diagnostic))
            {
                return false;
            }

            diagnostic = "STAGE_THEME_BACKGROUND_REGISTRY_VALID";
            return true;
        }

        public bool TryResolve(
            string profileId,
            out C1StageThemeBackgroundProfile profile,
            out string diagnostic)
        {
            profile = null;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_ID_MISSING";
                return false;
            }

            if (!TryValidate(out diagnostic))
            {
                return false;
            }

            if (string.Equals(
                    profileId,
                    InteriorProfileId,
                    StringComparison.Ordinal))
            {
                profile = interiorProfile;
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_RESOLVED profile=" +
                    profileId;
                return true;
            }

            if (string.Equals(
                    profileId,
                    ExteriorProfileId,
                    StringComparison.Ordinal))
            {
                profile = exteriorProfile;
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_RESOLVED profile=" +
                    profileId;
                return true;
            }

            if (string.Equals(
                    profileId,
                    NightAnimatedProfileId,
                    StringComparison.Ordinal))
            {
                profile = nightAnimatedProfile;
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_RESOLVED profile=" +
                    profileId;
                return true;
            }

            diagnostic =
                "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_UNKNOWN profile=" +
                profileId;
            return false;
        }

        public void ConfigureForEditor(
            C1StageThemeBackgroundProfile configuredInteriorProfile,
            C1StageThemeBackgroundProfile configuredExteriorProfile,
            C1StageThemeBackgroundProfile configuredNightAnimatedProfile)
        {
            interiorProfile = configuredInteriorProfile;
            exteriorProfile = configuredExteriorProfile;
            nightAnimatedProfile = configuredNightAnimatedProfile;
        }

        private static bool ValidateExactProfile(
            C1StageThemeBackgroundProfile profile,
            string slot,
            string expectedProfileId,
            out string diagnostic)
        {
            if (!string.Equals(
                    profile.ProfileId,
                    expectedProfileId,
                    StringComparison.Ordinal))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_ID_MISMATCH slot=" +
                    slot + " expected=" + expectedProfileId + " actual=" +
                    profile.ProfileId;
                return false;
            }

            if (!profile.TryValidate(out string profileDiagnostic))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_INVALID slot=" +
                    slot + " diagnostic=" + profileDiagnostic;
                return false;
            }

            diagnostic = "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_VALID";
            return true;
        }
    }
}
