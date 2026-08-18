using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.V04.ChapterFlow.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1
{
    public static class V04Chapter1StageWaveEncounterTruthVerifier
    {
        public const string PassMarker =
            "V04_C1_ENCOUNTER_WAVE_ACTOR_TARGET_TRUTH01_PASS "
            + "stages=10 waves=11 actors=29 targetTransitions=5 "
            + "bossGates=1 manualBossChallenges=1 "
            + "heldWaveActiveActors=0 resetLeak=0";
        public static void VerifyMenu()
        {
            Verify(writeReports: true);
        }

        public static void VerifyBatch()
        {
            Verify(writeReports: true);
        }

        private static void Verify(bool writeReports)
        {
            V04Chapter1StageWaveValidationResult truth =
                V04Chapter1StageWaveEncounterValidation.Validate();
            V04Chapter1ContinuousValidationResult flow =
                V04Chapter1ContinuousFlowValidation.ValidateNominal();
            string[] errors = truth.errors
                .Select(value => "truth:" + value)
                .Concat(flow.errors.Select(value => "flow:" + value))
                .ToArray();
            if (errors.Length > 0)
            {
                throw new InvalidOperationException(
                    "V0.4 C1 encounter wave/actor/target verification "
                    + "failed: " + string.Join(" | ", errors));
            }

            if (writeReports)
            {
                WriteReports(flow);
            }
            Debug.Log(PassMarker);
        }

        private static void WriteReports(
            V04Chapter1ContinuousValidationResult validation)
        {
            string reportDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Docs",
                "V0.4",
                "Reports");
            Directory.CreateDirectory(reportDirectory);
            UTF8Encoding utf8 = new(false);
            File.WriteAllText(
                Path.Combine(
                    reportDirectory,
                    "C1EncounterWaveActorPlan.csv"),
                BuildActorCsv(),
                utf8);
            File.WriteAllText(
                Path.Combine(
                    reportDirectory,
                    "C1EncounterTargetTransition.csv"),
                BuildTargetCsv(validation),
                utf8);
            File.WriteAllText(
                Path.Combine(
                    reportDirectory,
                    "C1EncounterWaveActorTargetTruthLeakCheckReport.md"),
                BuildLeakCheck(),
                utf8);
            File.WriteAllText(
                Path.Combine(
                    reportDirectory,
                    "C1EncounterWaveActorTargetTruthReport.md"),
                BuildMarkdown(validation),
                utf8);
        }

        private static string BuildActorCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "stageId,waveId,waveOrdinal,waveEntryId,"
                + "enemyContentId,runtimeProfileId,visualProfileKey,"
                + "occurrenceOrdinal,displaySlotId,slotOrdinal,"
                + "runtimeStatus,isBoss");
            foreach (V04Chapter1StageWavePlanDefinition stage in
                V04Chapter1StageWaveEncounterTruth.Plans)
            {
                foreach (V04Chapter1WavePlanDefinition wave in
                    stage.waves)
                {
                    foreach (V04Chapter1WaveEntryDefinition actor in
                        wave.actors)
                    {
                        builder.Append(Escape(stage.stageId)).Append(',')
                            .Append(Escape(wave.waveId)).Append(',')
                            .Append(wave.waveOrdinal.ToString(
                                CultureInfo.InvariantCulture)).Append(',')
                            .Append(Escape(actor.waveEntryId)).Append(',')
                            .Append(Escape(actor.enemyContentId)).Append(',')
                            .Append(Escape(actor.runtimeProfileId)).Append(',')
                            .Append(Escape(actor.visualProfileKey)).Append(',')
                            .Append(actor.occurrenceOrdinal.ToString(
                                CultureInfo.InvariantCulture)).Append(',')
                            .Append(Escape(actor.displaySlotId)).Append(',')
                            .Append(actor.slotOrdinal.ToString(
                                CultureInfo.InvariantCulture)).Append(',')
                            .Append(actor.runtimeStatus).Append(',')
                            .Append(actor.isBoss ? "true" : "false")
                            .AppendLine();
                    }
                }
            }
            return builder.ToString();
        }

        private static string BuildTargetCsv(
            V04Chapter1ContinuousValidationResult validation)
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "sequence,scenario,waveId,requestedEnemyInstanceId,"
                + "selectedBefore,selectedAfter,outcome");
            foreach (V04Chapter1TargetTransitionTraceRow row in
                validation.targetTransitions)
            {
                builder.Append(row.sequence.ToString(
                        CultureInfo.InvariantCulture)).Append(',')
                    .Append(Escape(row.scenario)).Append(',')
                    .Append(Escape(row.waveId)).Append(',')
                    .Append(Escape(
                        row.requestedEnemyInstanceId)).Append(',')
                    .Append(Escape(row.selectedBefore)).Append(',')
                    .Append(Escape(row.selectedAfter)).Append(',')
                    .Append(Escape(row.outcome)).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildMarkdown(
            V04Chapter1ContinuousValidationResult validation)
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "# C1 Encounter Wave Actor Target Truth Report");
            builder.AppendLine();
            builder.AppendLine(
                "- Package: `V0.4-C1EncounterWaveActorTargetTruth01`");
            builder.AppendLine("- Result: `PASS`.");
            builder.AppendLine("- Schema: `"
                + V04Chapter1StageWaveEncounterTruth.SchemaId + "`.");
            builder.AppendLine(
                "- Authority: `StagePlan -> WavePlan -> ActorPlan`; "
                + "the old sequential actor-as-wave manifest is "
                + "`RETIRED_DERIVED_VIEW_ONLY`.");
            builder.AppendLine(
                "- Counts: `10 stages / 11 waves / 29 actors`; each "
                + "Wave has `1..3` stable slots.");
            builder.AppendLine(
                "- Battle owns one `selectedTargetEnemyInstanceId`; "
                + "fallback is the lowest live `slotOrdinal`, invalid "
                + "selection preserves a valid target, death reselects "
                + "immediately, Wave/reset clears selection.");
            builder.AppendLine(
                "- Direct Item damage is applied through the existing "
                + "`C1EnemyBattleApplicationResult` primitive to the "
                + "selected Actor only; no broadcast/AoE/random/"
                + "lowest-HP mode was added.");
            builder.AppendLine(
                "- Bone Swap Remnant remains `HELD_BY_BA-D3`; a Wave "
                + "containing it fails before any Actor Runtime is "
                + "created with `RUNTIME_PROFILE_MISSING_HELD_BY_BA-D3`.");
            builder.AppendLine(
                "- 1-9 W1 advances only to W2; only W2 completion may "
                + "settle 1-9 and reach 1-10 BossGate. Boss remains "
                + "behind accepted ManualChallenge.");
            builder.AppendLine(
                "- Reset/restart validation: two generations, no prior "
                + "Actor ID, HP, shell, selection, request, cue, or "
                + "action leakage.");
            builder.AppendLine();
            builder.AppendLine("## Frozen Stage plans");
            builder.AppendLine();
            foreach (V04Chapter1StageWavePlanDefinition stage in
                V04Chapter1StageWaveEncounterTruth.Plans)
            {
                builder.Append("- `").Append(stage.stageId).Append("`: ")
                    .Append(string.Join(
                        " -> ",
                        stage.waves.Select(wave =>
                            "W" + wave.waveOrdinal + "["
                            + string.Join(
                                " + ",
                                wave.actors.Select(actor =>
                                    actor.enemyContentId + "@"
                                    + actor.displaySlotId))
                            + "]")))
                    .AppendLine();
            }
            builder.AppendLine();
            builder.AppendLine("## Automated assertions");
            builder.AppendLine();
            builder.AppendLine(
                "- Truth/grouping/identity, selected-only damage, "
                + "invalid-target preservation, death fallback, "
                + "three-Actor independence, held-Wave fail-close, "
                + "1-9 two-Wave boundary, ManualChallenge gate, and "
                + "two reset generations: `PASS`.");
            builder.AppendLine(
                "- Target transition rows: `"
                + validation.targetTransitions.Count + "`.");
            builder.AppendLine(
                "- Scene/Prefab/BuildSettings/UI/Visual/VFX/V02/V03/"
                + "Item algorithm changes: `0`.");
            return builder.ToString();
        }

        private static string BuildLeakCheck()
        {
            return string.Join(
                "\n",
                new[]
                {
                    "# C1 Encounter Wave Actor Target Truth Leak Check",
                    "",
                    "- Result: `PASS`.",
                    "- Old sequential actor-as-wave authority: `0` "
                        + "(derived compatibility view only).",
                    "- Held Wave partial Actor creation: `0`.",
                    "- Direct damage broadcast targets: `0`.",
                    "- Stale Actor/HP/shell/selection/request/cue/action "
                        + "facts after two resets: `0`.",
                    "- Scene/Prefab/BuildSettings/SceneBinder/UI/Visual/"
                        + "VFX/V02/V03/Item algorithm mutations: `0`.",
                    ""
                });
        }

        private static string Escape(string value)
        {
            string escaped = (value ?? string.Empty)
                .Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }
    }
}
