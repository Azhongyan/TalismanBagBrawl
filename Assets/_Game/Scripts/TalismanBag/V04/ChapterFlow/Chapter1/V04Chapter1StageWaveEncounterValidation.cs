using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;

namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public sealed class V04Chapter1StageWaveValidationResult
    {
        public readonly List<string> errors = new();
        public bool Passed => errors.Count == 0;

        public void Require(bool condition, string code)
        {
            if (!condition)
            {
                errors.Add(code ?? "UNKNOWN");
            }
        }
    }

    public static class V04Chapter1StageWaveEncounterValidation
    {
        private static string Host =>
            C1EnemyRuntimeContract.ShatteredHostContentId;
        private static string Hound =>
            C1EnemyRuntimeContract.PorcelainHoundContentId;
        private static string Remnant =>
            C1EnemyRuntimeContract.BoneSwapRemnantContentId;
        private static string Boss =>
            ShougunuPhase1RuntimeContract.ContentId;

        public static V04Chapter1StageWaveValidationResult Validate()
        {
            V04Chapter1StageWaveValidationResult result = new();
            IReadOnlyList<V04Chapter1StageWavePlanDefinition> stages =
                V04Chapter1StageWaveEncounterTruth.Plans;
            IReadOnlyList<V04Chapter1WavePlanDefinition> waves =
                V04Chapter1StageWaveEncounterTruth.Waves;
            IReadOnlyList<V04Chapter1WaveEntryDefinition> actors =
                V04Chapter1StageWaveEncounterTruth.Entries;

            result.Require(
                V04Chapter1StageWaveEncounterTruth.SchemaId ==
                    "V04Chapter1EncounterWaveActorTargetTruth.v2",
                "SCHEMA_ID_V2");
            result.Require(
                V04Chapter1StageWaveEncounterTruth.AuthorityMode ==
                    "STAGE_PLAN_TO_WAVE_PLAN_TO_ACTOR_PLAN_ONLY"
                && V04Chapter1StageWaveEncounterTruth
                    .LegacySequentialManifestStatus ==
                    "RETIRED_DERIVED_VIEW_ONLY",
                "SINGLE_AUTHORITY_OLD_SEQUENTIAL_RETIRED");
            result.Require(stages.Count == 10, "STAGE_COUNT_10");
            result.Require(waves.Count == 11, "WAVE_COUNT_11");
            result.Require(actors.Count == 29, "ACTOR_COUNT_29");
            result.Require(
                waves.Select(value => value.waveId)
                    .Distinct(StringComparer.Ordinal).Count() == waves.Count,
                "WAVE_ID_UNIQUE");
            result.Require(
                actors.Select(value => value.waveEntryId)
                    .Distinct(StringComparer.Ordinal).Count() == actors.Count,
                "WAVE_ENTRY_ID_UNIQUE");
            result.Require(
                actors.All(value => value.slotOrdinal >= 1
                    && value.slotOrdinal <= 3),
                "SLOT_ORDINAL_RANGE_1_3");

            ValidateStage(
                result,
                "1-1",
                new[] { new[] { Host, Host } });
            ValidateStage(
                result,
                "1-2",
                new[] { new[] { Host, Host, Hound } });
            ValidateStage(
                result,
                "1-3",
                new[] { new[] { Host, Hound, Hound } });
            ValidateStage(
                result,
                "1-4",
                new[] { new[] { Hound, Remnant } });
            ValidateStage(
                result,
                "1-5",
                new[] { new[] { Host, Hound, Remnant } });
            ValidateStage(
                result,
                "1-6",
                new[] { new[] { Host, Host, Remnant } });
            ValidateStage(
                result,
                "1-7",
                new[] { new[] { Hound, Hound, Remnant } });
            ValidateStage(
                result,
                "1-8",
                new[] { new[] { Host, Remnant, Remnant } });
            ValidateStage(
                result,
                "1-9",
                new[]
                {
                    new[] { Host, Host, Hound },
                    new[] { Hound, Remnant, Remnant }
                });
            ValidateStage(
                result,
                "1-10",
                new[] { new[] { Boss } });

            ValidateRuntimeIdentities(result, actors);
            return result;
        }

        private static void ValidateStage(
            V04Chapter1StageWaveValidationResult result,
            string stageId,
            IReadOnlyList<string[]> expectedWaves)
        {
            V04Chapter1StageWavePlanDefinition stage =
                V04Chapter1StageWaveEncounterTruth.FindPlan(stageId);
            result.Require(stage != null, "STAGE_MISSING_" + stageId);
            if (stage == null)
            {
                return;
            }
            result.Require(
                stage.waves.Count == expectedWaves.Count,
                "WAVE_COUNT_" + stageId);

            Dictionary<string, int> occurrence =
                new(StringComparer.Ordinal);
            for (int waveIndex = 0;
                waveIndex < Math.Min(stage.waves.Count, expectedWaves.Count);
                waveIndex++)
            {
                V04Chapter1WavePlanDefinition wave =
                    stage.waves[waveIndex];
                string[] expectedActors = expectedWaves[waveIndex];
                string suffix = stageId + "_W"
                    + (waveIndex + 1).ToString(
                        CultureInfo.InvariantCulture);
                result.Require(
                    wave.stageId == stageId
                    && wave.waveOrdinal == waveIndex + 1
                    && wave.waveId == string.Format(
                        CultureInfo.InvariantCulture,
                        "v04.c1.wave.{0}.w{1:D2}",
                        stageId,
                        waveIndex + 1),
                    "WAVE_IDENTITY_" + suffix);
                result.Require(
                    wave.actors.Count >= 1 && wave.actors.Count <= 3,
                    "ACTOR_COUNT_RANGE_" + suffix);
                result.Require(
                    wave.actors.Select(value => value.enemyContentId)
                        .SequenceEqual(expectedActors),
                    "ACTOR_GROUP_" + suffix);

                for (int actorIndex = 0;
                    actorIndex < wave.actors.Count;
                    actorIndex++)
                {
                    V04Chapter1WaveEntryDefinition actor =
                        wave.actors[actorIndex];
                    occurrence.TryGetValue(
                        actor.enemyContentId,
                        out int previousOccurrence);
                    int expectedOccurrence = previousOccurrence + 1;
                    occurrence[actor.enemyContentId] = expectedOccurrence;
                    int expectedSlot = actorIndex + 1;
                    string actorSuffix = suffix + "_S" + expectedSlot;

                    result.Require(
                        actor.stageId == stageId
                        && actor.waveId == wave.waveId
                        && actor.waveOrdinal == wave.waveOrdinal,
                        "ACTOR_WAVE_CORRELATION_" + actorSuffix);
                    result.Require(
                        actor.slotOrdinal == expectedSlot
                        && actor.displaySlotId == string.Format(
                            CultureInfo.InvariantCulture,
                            "Slot{0:D2}",
                            expectedSlot),
                        "ACTOR_SLOT_" + actorSuffix);
                    result.Require(
                        actor.occurrenceOrdinal == expectedOccurrence,
                        "ACTOR_OCCURRENCE_" + actorSuffix);
                    result.Require(
                        !string.IsNullOrWhiteSpace(actor.waveEntryId)
                        && !string.IsNullOrWhiteSpace(actor.enemyContentId)
                        && !string.IsNullOrWhiteSpace(actor.runtimeProfileId)
                        && !string.IsNullOrWhiteSpace(
                            actor.visualProfileKey),
                        "ACTOR_REQUIRED_FIELDS_" + actorSuffix);
                    result.Require(
                        actor.waveEntryId.Contains(
                            ".slot" + expectedSlot.ToString(
                                "D2",
                                CultureInfo.InvariantCulture)
                            + ".",
                            StringComparison.Ordinal),
                        "ACTOR_STABLE_SLOT_ID_" + actorSuffix);
                }
            }
        }

        private static void ValidateRuntimeIdentities(
            V04Chapter1StageWaveValidationResult result,
            IReadOnlyList<V04Chapter1WaveEntryDefinition> actors)
        {
            IReadOnlyList<C1EnemyRuntimeProfileSnapshot> profiles =
                C1EnemyRuntimeCatalog.GetProfiles();
            foreach (V04Chapter1WaveEntryDefinition actor in actors)
            {
                if (actor.enemyContentId == Host
                    || actor.enemyContentId == Hound)
                {
                    C1EnemyRuntimeProfileSnapshot profile =
                        profiles.SingleOrDefault(value =>
                            value.ContentId == actor.enemyContentId
                            && value.RuntimeProfileId ==
                                actor.runtimeProfileId);
                    result.Require(
                        profile != null
                        && profile.PresentationKey ==
                            actor.visualProfileKey
                        && actor.runtimeStatus ==
                            V04Chapter1WaveRuntimeStatus.Available
                        && !actor.isBoss,
                        "REAL_RUNTIME_" + actor.waveEntryId);
                }
                else if (actor.enemyContentId == Remnant)
                {
                    result.Require(
                        actor.runtimeProfileId ==
                            V04Chapter1StageWaveEncounterTruth
                                .BoneSwapRemnantRuntimeHoldIdentity
                        && actor.runtimeStatus ==
                            V04Chapter1WaveRuntimeStatus.HeldByBad3
                        && actor.visualProfileKey ==
                            V04Chapter1StageWaveEncounterTruth
                                .BoneSwapRemnantVisualProfileKey
                        && !actor.visualProfileKey.Contains(
                            "d1_3",
                            StringComparison.OrdinalIgnoreCase)
                        && profiles.All(value =>
                            value.ContentId != actor.enemyContentId),
                        "REMNANT_HOLD_" + actor.waveEntryId);
                }
                else if (actor.enemyContentId == Boss)
                {
                    result.Require(
                        actor.stageId == "1-10"
                        && actor.runtimeProfileId ==
                            ShougunuPhase1RuntimeContract.PhaseId
                        && actor.visualProfileKey ==
                            ShougunuPhase1RuntimeContract.PresentationKey
                        && actor.isBoss,
                        "BOSS_IDENTITY");
                }
                else
                {
                    result.Require(
                        false,
                        "UNKNOWN_CONTENT_" + actor.waveEntryId);
                }
            }
        }
    }
}
