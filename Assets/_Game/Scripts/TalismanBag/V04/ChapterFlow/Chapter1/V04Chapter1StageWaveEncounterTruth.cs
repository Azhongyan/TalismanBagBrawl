using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;

namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public enum V04Chapter1WaveRuntimeStatus
    {
        Available = 0,
        HeldByBad3 = 1
    }

    public class V04Chapter1ActorPlanDefinition
    {
        protected internal V04Chapter1ActorPlanDefinition(
            string waveEntryId,
            string waveId,
            string stageId,
            int waveOrdinal,
            string enemyContentId,
            string runtimeProfileId,
            string visualProfileKey,
            int occurrenceOrdinal,
            string displaySlotId,
            int slotOrdinal,
            V04Chapter1WaveRuntimeStatus runtimeStatus,
            bool isBoss)
        {
            this.waveEntryId = waveEntryId ?? string.Empty;
            this.waveId = waveId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            this.waveOrdinal = waveOrdinal;
            this.enemyContentId = enemyContentId ?? string.Empty;
            this.runtimeProfileId = runtimeProfileId ?? string.Empty;
            this.visualProfileKey = visualProfileKey ?? string.Empty;
            this.occurrenceOrdinal = occurrenceOrdinal;
            this.displaySlotId = displaySlotId ?? string.Empty;
            this.slotOrdinal = slotOrdinal;
            this.runtimeStatus = runtimeStatus;
            this.isBoss = isBoss;
        }

        public readonly string waveEntryId;
        public readonly string waveId;
        public readonly string stageId;
        public readonly int waveOrdinal;
        public readonly string enemyContentId;
        public readonly string runtimeProfileId;
        public readonly string visualProfileKey;
        public readonly int occurrenceOrdinal;
        public readonly string displaySlotId;
        public readonly int slotOrdinal;
        public readonly V04Chapter1WaveRuntimeStatus runtimeStatus;
        public readonly bool isBoss;

        public bool HasRuntimeProfile =>
            runtimeStatus == V04Chapter1WaveRuntimeStatus.Available;
    }

    // Compatibility shape only. These rows are the ActorPlan instances owned by
    // the v2 Stage -> Wave -> Actor truth; they are not a second manifest.
    public sealed class V04Chapter1WaveEntryDefinition :
        V04Chapter1ActorPlanDefinition
    {
        internal V04Chapter1WaveEntryDefinition(
            string waveEntryId,
            string waveId,
            string stageId,
            int waveOrdinal,
            string enemyContentId,
            string runtimeProfileId,
            string visualProfileKey,
            int occurrenceOrdinal,
            string displaySlotId,
            int slotOrdinal,
            V04Chapter1WaveRuntimeStatus runtimeStatus,
            bool isBoss)
            : base(
                waveEntryId,
                waveId,
                stageId,
                waveOrdinal,
                enemyContentId,
                runtimeProfileId,
                visualProfileKey,
                occurrenceOrdinal,
                displaySlotId,
                slotOrdinal,
                runtimeStatus,
                isBoss)
        {
        }

        public int waveIndex => waveOrdinal;
    }

    public sealed class V04Chapter1WavePlanDefinition
    {
        internal V04Chapter1WavePlanDefinition(
            string waveId,
            string stageId,
            int waveOrdinal,
            IReadOnlyList<V04Chapter1WaveEntryDefinition> actors)
        {
            this.waveId = waveId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            this.waveOrdinal = waveOrdinal;
            this.actors = actors ??
                Array.Empty<V04Chapter1WaveEntryDefinition>();
        }

        public readonly string waveId;
        public readonly string stageId;
        public readonly int waveOrdinal;
        public readonly IReadOnlyList<V04Chapter1WaveEntryDefinition> actors;

        public bool isBoss => actors.Count == 1 && actors[0].isBoss;
        public bool HasAllRuntimeProfiles =>
            actors.Count > 0 && actors.All(value => value.HasRuntimeProfile);
        public bool HasHeldRuntime =>
            actors.Any(value => !value.HasRuntimeProfile);
        public string CompatibilityRequestProfileId =>
            actors.FirstOrDefault()?.runtimeProfileId ?? string.Empty;
    }

    public sealed class V04Chapter1StageWavePlanDefinition
    {
        internal V04Chapter1StageWavePlanDefinition(
            string stageId,
            IReadOnlyList<V04Chapter1WavePlanDefinition> waves)
        {
            this.stageId = stageId ?? string.Empty;
            this.waves = waves ??
                Array.Empty<V04Chapter1WavePlanDefinition>();
            entries = this.waves
                .SelectMany(value => value.actors)
                .ToArray();
        }

        public readonly string stageId;
        public readonly IReadOnlyList<V04Chapter1WavePlanDefinition> waves;

        // Retained as a read-only derived compatibility view for older probes.
        // Runtime progression must use waves, never this flattened list.
        public readonly IReadOnlyList<V04Chapter1WaveEntryDefinition> entries;
    }

    public static class V04Chapter1StageWaveEncounterTruth
    {
        public const string SchemaId =
            "V04Chapter1EncounterWaveActorTargetTruth.v2";
        public const string AuthorityMode =
            "STAGE_PLAN_TO_WAVE_PLAN_TO_ACTOR_PLAN_ONLY";
        public const string LegacySequentialManifestStatus =
            "RETIRED_DERIVED_VIEW_ONLY";
        public const string RuntimeProfileMissingHeldByBad3 =
            "RUNTIME_PROFILE_MISSING_HELD_BY_BA-D3";
        public const string BoneSwapRemnantRuntimeHoldIdentity =
            "HELD_BY_BA-D3";
        public const string BoneSwapRemnantVisualProfileKey =
            "enemy.bone_aspect.c1.bone_swap_remnant";

        private static readonly IReadOnlyList<
            V04Chapter1StageWavePlanDefinition> PlansInternal =
                new[]
                {
                    Build("1-1", Wave(Host(), Host())),
                    Build("1-2", Wave(Host(), Host(), Host())),
                    Build("1-3", Wave(Host(), Hound())),
                    Build("1-4", Wave(Hound(), Remnant())),
                    Build("1-5", Wave(Host(), Hound(), Remnant())),
                    Build("1-6", Wave(Host(), Host(), Host(), Remnant())),
                    Build("1-7", Wave(Hound(), Hound(), Remnant())),
                    Build("1-8", Wave(Host(), Host(), Remnant(), Remnant())),
                    Build(
                        "1-9",
                        Wave(Host(), Host(), Host(), Hound()),
                        Wave(Hound(), Remnant(), Remnant())),
                    Build("1-10", Wave(Boss()))
                };

        public static IReadOnlyList<V04Chapter1StageWavePlanDefinition>
            Plans => PlansInternal;

        public static IReadOnlyList<V04Chapter1WavePlanDefinition> Waves =>
            PlansInternal.SelectMany(value => value.waves).ToArray();

        public static IReadOnlyList<V04Chapter1WaveEntryDefinition>
            Entries => PlansInternal.SelectMany(value => value.entries)
                .ToArray();

        public static IReadOnlyList<V04Chapter1ActorPlanDefinition> Actors =>
            Entries;

        public static V04Chapter1StageWavePlanDefinition FindPlan(
            string stageId)
        {
            return PlansInternal.SingleOrDefault(value =>
                string.Equals(
                    value.stageId,
                    stageId,
                    StringComparison.Ordinal));
        }

        public static V04Chapter1WavePlanDefinition FindWave(
            string waveId)
        {
            return Waves.SingleOrDefault(value => string.Equals(
                value.waveId,
                waveId,
                StringComparison.Ordinal));
        }

        public static V04Chapter1ActorPlanDefinition FindActor(
            string waveEntryId)
        {
            return Entries.SingleOrDefault(value => string.Equals(
                value.waveEntryId,
                waveEntryId,
                StringComparison.Ordinal));
        }

        public static V04Chapter1WaveEntryDefinition FindEntry(
            string waveEntryId)
        {
            return Entries.SingleOrDefault(value => string.Equals(
                value.waveEntryId,
                waveEntryId,
                StringComparison.Ordinal));
        }

        private static V04Chapter1StageWavePlanDefinition Build(
            string stageId,
            params WaveIdentity[] waveIdentities)
        {
            Dictionary<string, int> occurrenceByContent =
                new(StringComparer.Ordinal);
            List<V04Chapter1WavePlanDefinition> waves = new();
            for (int waveIndex = 0; waveIndex < waveIdentities.Length;
                waveIndex++)
            {
                int waveOrdinal = waveIndex + 1;
                string waveId = string.Format(
                    CultureInfo.InvariantCulture,
                    "v04.c1.wave.{0}.w{1:D2}",
                    stageId,
                    waveOrdinal);
                List<V04Chapter1WaveEntryDefinition> actors = new();
                ActorIdentity[] identities =
                    waveIdentities[waveIndex].Actors;
                for (int actorIndex = 0; actorIndex < identities.Length;
                    actorIndex++)
                {
                    ActorIdentity identity = identities[actorIndex];
                    occurrenceByContent.TryGetValue(
                        identity.EnemyContentId,
                        out int previousOccurrence);
                    int occurrenceOrdinal = previousOccurrence + 1;
                    occurrenceByContent[identity.EnemyContentId] =
                        occurrenceOrdinal;
                    int slotOrdinal = actorIndex + 1;
                    string displaySlotId = string.Format(
                        CultureInfo.InvariantCulture,
                        "Slot{0:D2}",
                        slotOrdinal);
                    string waveEntryId = string.Format(
                        CultureInfo.InvariantCulture,
                        "v04.c1.actor.{0}.w{1:D2}.slot{2:D2}.{3}.o{4:D2}",
                        stageId,
                        waveOrdinal,
                        slotOrdinal,
                        identity.StableToken,
                        occurrenceOrdinal);
                    actors.Add(new V04Chapter1WaveEntryDefinition(
                        waveEntryId,
                        waveId,
                        stageId,
                        waveOrdinal,
                        identity.EnemyContentId,
                        identity.RuntimeProfileId,
                        identity.VisualProfileKey,
                        occurrenceOrdinal,
                        displaySlotId,
                        slotOrdinal,
                        identity.RuntimeStatus,
                        identity.IsBoss));
                }
                waves.Add(new V04Chapter1WavePlanDefinition(
                    waveId,
                    stageId,
                    waveOrdinal,
                    actors.ToArray()));
            }
            return new V04Chapter1StageWavePlanDefinition(
                stageId,
                waves.ToArray());
        }

        private static WaveIdentity Wave(params ActorIdentity[] actors)
        {
            return new WaveIdentity(actors);
        }

        private static ActorIdentity Host()
        {
            return new ActorIdentity(
                "shattered_host",
                C1EnemyRuntimeContract.ShatteredHostContentId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                C1EnemyRuntimeContract.ShatteredHostPresentationKey,
                V04Chapter1WaveRuntimeStatus.Available,
                false);
        }

        private static ActorIdentity Hound()
        {
            return new ActorIdentity(
                "porcelain_hound",
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                C1EnemyRuntimeContract.PorcelainHoundPresentationKey,
                V04Chapter1WaveRuntimeStatus.Available,
                false);
        }

        private static ActorIdentity Remnant()
        {
            return new ActorIdentity(
                "bone_swap_remnant",
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
                    .BoneSwapRemnant.BoneSwapRemnantRuntimeContract
                    .OperatorProfileId,
                BoneSwapRemnantVisualProfileKey,
                V04Chapter1WaveRuntimeStatus.Available,
                false);
        }

        private static ActorIdentity Boss()
        {
            return new ActorIdentity(
                "shougunu",
                ShougunuPhase1RuntimeContract.ContentId,
                ShougunuPhase1RuntimeContract.PhaseId,
                ShougunuPhase1RuntimeContract.PresentationKey,
                V04Chapter1WaveRuntimeStatus.Available,
                true);
        }

        private sealed class WaveIdentity
        {
            public WaveIdentity(ActorIdentity[] actors)
            {
                Actors = actors ?? Array.Empty<ActorIdentity>();
            }

            public ActorIdentity[] Actors { get; }
        }

        private sealed class ActorIdentity
        {
            public ActorIdentity(
                string stableToken,
                string enemyContentId,
                string runtimeProfileId,
                string visualProfileKey,
                V04Chapter1WaveRuntimeStatus runtimeStatus,
                bool isBoss)
            {
                StableToken = stableToken;
                EnemyContentId = enemyContentId;
                RuntimeProfileId = runtimeProfileId;
                VisualProfileKey = visualProfileKey;
                RuntimeStatus = runtimeStatus;
                IsBoss = isBoss;
            }

            public string StableToken { get; }
            public string EnemyContentId { get; }
            public string RuntimeProfileId { get; }
            public string VisualProfileKey { get; }
            public V04Chapter1WaveRuntimeStatus RuntimeStatus { get; }
            public bool IsBoss { get; }
        }
    }
}
