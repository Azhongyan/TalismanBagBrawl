using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.Normalization;

namespace TalismanBag.EnemySystem.Composition
{
    public sealed class EncounterCompositionValidationIssue
    {
        public EncounterCompositionValidationIssue(string code, string path, string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public interface IEncounterCompositionValidator
    {
        IReadOnlyList<EncounterCompositionValidationIssue> Validate(
            EncounterCompositionCatalogInput input,
            IEncounterCompositionReferenceResolver resolver);
    }

    public sealed class EncounterCompositionValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EncounterCompositionValidationIssue> issues;

        public EncounterCompositionValidationException(IReadOnlyList<EncounterCompositionValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<EncounterCompositionValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EncounterCompositionValidationIssue> Issues => issues;

        private static string BuildMessage(IReadOnlyList<EncounterCompositionValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder("Encounter composition validation failed.");
            foreach (EncounterCompositionValidationIssue issue
                in issues ?? Array.Empty<EncounterCompositionValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultEncounterCompositionProvider : IEncounterCompositionProvider
    {
        public static readonly DefaultEncounterCompositionProvider Instance =
            new DefaultEncounterCompositionProvider();

        private readonly IEncounterCompositionValidator validator;

        public DefaultEncounterCompositionProvider(IEncounterCompositionValidator validator = null)
        {
            this.validator = validator ?? DefaultEncounterCompositionValidator.Instance;
        }

        public EncounterCompositionCatalogSnapshot CreateSnapshot(
            EncounterCompositionCatalogInput input,
            IEncounterCompositionReferenceResolver resolver)
        {
            IReadOnlyList<EncounterCompositionValidationIssue> issues =
                validator.Validate(input, resolver);
            if (issues.Count > 0)
            {
                throw new EncounterCompositionValidationException(issues);
            }

            return new EncounterCompositionCatalogSnapshot(input);
        }
    }

    public sealed class DefaultEncounterCompositionValidator : IEncounterCompositionValidator
    {
        public static readonly DefaultEncounterCompositionValidator Instance =
            new DefaultEncounterCompositionValidator();

        public IReadOnlyList<EncounterCompositionValidationIssue> Validate(
            EncounterCompositionCatalogInput input,
            IEncounterCompositionReferenceResolver resolver)
        {
            List<EncounterCompositionValidationIssue> issues =
                new List<EncounterCompositionValidationIssue>();
            if (input == null)
            {
                issues.Add(Issue("INPUT_NULL", "$", "Catalog input is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(
                input.SchemaId,
                EncounterCompositionSchema.SchemaId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "SCHEMA_ID_MISMATCH",
                    "schemaId",
                    "Schema ID must match EncounterComposition.v1 exactly."));
            }

            if (input.SchemaVersion != EncounterCompositionSchema.SchemaVersion)
            {
                issues.Add(Issue(
                    "SCHEMA_VERSION_MISMATCH",
                    "schemaVersion",
                    "Schema version must equal 1."));
            }

            if (resolver == null)
            {
                issues.Add(Issue(
                    "REFERENCE_RESOLVER_NULL",
                    "$",
                    "A read-only E03 reference resolver is required."));
            }

            Dictionary<string, int> firstEncounterById =
                new Dictionary<string, int>(StringComparer.Ordinal);
            for (int encounterIndex = 0;
                encounterIndex < input.Encounters.Count;
                encounterIndex++)
            {
                EncounterCompositionSnapshot encounter = input.Encounters[encounterIndex];
                string encounterPath = "encounters[" + encounterIndex + "]";
                if (encounter == null)
                {
                    issues.Add(Issue(
                        "ENCOUNTER_NULL",
                        encounterPath,
                        "Encounter entries cannot be null."));
                    continue;
                }

                ValidateEncounter(
                    encounter,
                    encounterPath,
                    resolver,
                    issues);

                string encounterId = encounter.EncounterId;
                if (firstEncounterById.TryGetValue(encounterId, out int firstIndex))
                {
                    issues.Add(Issue(
                        "ENCOUNTER_ID_DUPLICATE",
                        encounterPath + ".encounterReference.stableId",
                        "Duplicate exact Encounter ID; first seen at index " + firstIndex + "."));
                }
                else
                {
                    firstEncounterById.Add(encounterId, encounterIndex);
                }
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static void ValidateEncounter(
            EncounterCompositionSnapshot encounter,
            string path,
            IEncounterCompositionReferenceResolver resolver,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            if (encounter.EncounterReference == null)
            {
                issues.Add(Issue(
                    "ENCOUNTER_REFERENCE_NULL",
                    path + ".encounterReference",
                    "E01 EncounterReference is required."));
            }
            else
            {
                ValidateId(
                    encounter.EncounterReference.StableId,
                    path + ".encounterReference.stableId",
                    issues);
                if (!encounter.EncounterReference.DevOnly
                    || encounter.EncounterReference.IsEnabled
                    || encounter.EncounterReference.EntersFormalFlow)
                {
                    issues.Add(Issue(
                        "DEV_ISOLATION_INVALID",
                        path + ".encounterReference",
                        "Required isolation is devOnly=true, isEnabled=false, entersFormalFlow=false."));
                }
            }

            ValidateMapRules(encounter, path, resolver, issues);
            ValidateDeveloperTags(encounter, path, issues);

            if (encounter.Waves.Count == 0)
            {
                issues.Add(Issue(
                    "ENCOUNTER_WAVES_EMPTY",
                    path + ".waves",
                    "An Encounter requires at least one Wave."));
                return;
            }

            Dictionary<string, int> firstWaveById =
                new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<int, int> firstWaveByOrder = new Dictionary<int, int>();
            Dictionary<string, string> firstSlotPathById =
                new Dictionary<string, string>(StringComparer.Ordinal);
            for (int waveIndex = 0; waveIndex < encounter.Waves.Count; waveIndex++)
            {
                EncounterWaveSnapshot wave = encounter.Waves[waveIndex];
                string wavePath = path + ".waves[" + waveIndex + "]";
                if (wave == null)
                {
                    issues.Add(Issue(
                        "WAVE_NULL",
                        wavePath,
                        "Wave entries cannot be null."));
                    continue;
                }

                ValidateId(wave.WaveId, wavePath + ".waveId", issues);
                if (firstWaveById.TryGetValue(wave.WaveId, out int firstWaveIndex))
                {
                    issues.Add(Issue(
                        "WAVE_ID_DUPLICATE",
                        wavePath + ".waveId",
                        "Duplicate exact Wave ID; first seen at index " + firstWaveIndex + "."));
                }
                else
                {
                    firstWaveById.Add(wave.WaveId, waveIndex);
                }

                if (wave.WaveOrder < 0)
                {
                    issues.Add(Issue(
                        "WAVE_ORDER_NEGATIVE",
                        wavePath + ".waveOrder",
                        "WaveOrder must be zero-based and non-negative."));
                }

                if (firstWaveByOrder.TryGetValue(wave.WaveOrder, out int firstOrderIndex))
                {
                    issues.Add(Issue(
                        "WAVE_ORDER_DUPLICATE",
                        wavePath + ".waveOrder",
                        "Duplicate WaveOrder; first seen at index " + firstOrderIndex + "."));
                }
                else
                {
                    firstWaveByOrder.Add(wave.WaveOrder, waveIndex);
                }

                ValidateWave(
                    wave,
                    wavePath,
                    resolver,
                    firstSlotPathById,
                    issues);
            }

            ValidateContiguousOrder(
                encounter.Waves
                    .Where(value => value != null)
                    .Select(value => value.WaveOrder),
                path + ".waves",
                "WAVE_ORDER_NON_CONTIGUOUS",
                "WaveOrder must be exactly 0..N-1.",
                issues);
        }

        private static void ValidateMapRules(
            EncounterCompositionSnapshot encounter,
            string path,
            IEncounterCompositionReferenceResolver resolver,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < encounter.MapRuleIds.Count; index++)
            {
                string id = encounter.MapRuleIds[index];
                string idPath = path + ".mapRuleIds[" + index + "]";
                ValidateId(id, idPath, issues);
                if (!seen.Add(id))
                {
                    issues.Add(Issue(
                        "MAP_RULE_ID_DUPLICATE",
                        idPath,
                        "An Encounter cannot repeat the same exact MapRule ID."));
                }

                if (resolver != null && !resolver.TryGetMapRule(id, out _))
                {
                    issues.Add(Issue(
                        "MAP_RULE_REFERENCE_UNRESOLVED",
                        idPath,
                        "MapRule ID does not resolve with ordinal semantics."));
                }
            }
        }

        private static void ValidateDeveloperTags(
            EncounterCompositionSnapshot encounter,
            string path,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            for (int index = 0; index < encounter.DeveloperContentTagIds.Count; index++)
            {
                ValidateId(
                    encounter.DeveloperContentTagIds[index],
                    path + ".developerContentTagIds[" + index + "]",
                    issues);
            }
        }

        private static void ValidateWave(
            EncounterWaveSnapshot wave,
            string path,
            IEncounterCompositionReferenceResolver resolver,
            IDictionary<string, string> firstSlotPathById,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            if (wave.Slots.Count == 0)
            {
                issues.Add(Issue(
                    "WAVE_SLOTS_EMPTY",
                    path + ".slots",
                    "A Wave requires at least one Slot."));
                return;
            }

            Dictionary<int, int> firstSlotByOrder = new Dictionary<int, int>();
            for (int slotIndex = 0; slotIndex < wave.Slots.Count; slotIndex++)
            {
                EncounterSlotSnapshot slot = wave.Slots[slotIndex];
                string slotPath = path + ".slots[" + slotIndex + "]";
                if (slot == null)
                {
                    issues.Add(Issue(
                        "SLOT_NULL",
                        slotPath,
                        "Slot entries cannot be null."));
                    continue;
                }

                ValidateId(slot.SlotId, slotPath + ".slotId", issues);
                if (firstSlotPathById.TryGetValue(slot.SlotId, out string firstPath))
                {
                    issues.Add(Issue(
                        "SLOT_ID_DUPLICATE",
                        slotPath + ".slotId",
                        "Duplicate exact Slot ID in Encounter; first seen at " + firstPath + "."));
                }
                else
                {
                    firstSlotPathById.Add(slot.SlotId, slotPath);
                }

                if (slot.SpawnOrder < 0)
                {
                    issues.Add(Issue(
                        "SPAWN_ORDER_NEGATIVE",
                        slotPath + ".spawnOrder",
                        "SpawnOrder must be zero-based and non-negative."));
                }

                if (firstSlotByOrder.TryGetValue(slot.SpawnOrder, out int firstOrderIndex))
                {
                    issues.Add(Issue(
                        "SPAWN_ORDER_DUPLICATE",
                        slotPath + ".spawnOrder",
                        "Duplicate SpawnOrder; first seen at index " + firstOrderIndex + "."));
                }
                else
                {
                    firstSlotByOrder.Add(slot.SpawnOrder, slotIndex);
                }

                ValidateSlot(slot, slotPath, resolver, issues);
            }

            ValidateContiguousOrder(
                wave.Slots
                    .Where(value => value != null)
                    .Select(value => value.SpawnOrder),
                path + ".slots",
                "SPAWN_ORDER_NON_CONTIGUOUS",
                "SpawnOrder must be exactly 0..N-1 inside each Wave.",
                issues);
        }

        private static void ValidateSlot(
            EncounterSlotSnapshot slot,
            string path,
            IEncounterCompositionReferenceResolver resolver,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            ValidateId(slot.CarrierId, path + ".carrierId", issues);
            if (slot.Quantity < 1)
            {
                issues.Add(Issue(
                    "QUANTITY_INVALID",
                    path + ".quantity",
                    "Quantity must be at least 1."));
            }

            bool kindDefined = Enum.IsDefined(typeof(EncounterSlotKind), slot.Kind);
            bool roleDefined = Enum.IsDefined(typeof(EncounterSlotRole), slot.Role);
            if (!kindDefined)
            {
                issues.Add(Issue(
                    "SLOT_KIND_INVALID",
                    path + ".kind",
                    "EncounterSlotKind must be Enemy or Boss."));
            }

            if (!roleDefined)
            {
                issues.Add(Issue(
                    "SLOT_ROLE_INVALID",
                    path + ".role",
                    "EncounterSlotRole must be Normal, Elite, or Boss."));
            }

            if (kindDefined
                && slot.Kind == EncounterSlotKind.Enemy
                && roleDefined
                && slot.Role == EncounterSlotRole.Boss)
            {
                issues.Add(Issue(
                    "ENEMY_ROLE_INVALID",
                    path + ".role",
                    "Enemy slots can only be Normal or Elite."));
            }

            if (kindDefined
                && slot.Kind == EncounterSlotKind.Boss
                && roleDefined
                && slot.Role != EncounterSlotRole.Boss)
            {
                issues.Add(Issue(
                    "BOSS_ROLE_INVALID",
                    path + ".role",
                    "Boss slots must use the Boss role."));
            }

            bool carrierResolved = false;
            if (resolver != null && kindDefined)
            {
                bool enemyExists = resolver.TryGetEnemy(slot.CarrierId, out _);
                bool bossExists = resolver.TryGetBoss(slot.CarrierId, out _);
                carrierResolved = slot.Kind == EncounterSlotKind.Enemy
                    ? enemyExists
                    : bossExists;
                if (!carrierResolved)
                {
                    issues.Add(Issue(
                        slot.Kind == EncounterSlotKind.Enemy
                            ? "ENEMY_REFERENCE_UNRESOLVED"
                            : "BOSS_REFERENCE_UNRESOLVED",
                        path + ".carrierId",
                        "Carrier ID does not resolve for the declared Slot kind."));
                }

                if ((slot.Kind == EncounterSlotKind.Enemy && bossExists)
                    || (slot.Kind == EncounterSlotKind.Boss && enemyExists))
                {
                    issues.Add(Issue(
                        "SLOT_CARRIER_KIND_MISMATCH",
                        path + ".carrierId",
                        "Carrier exists only under the opposite Enemy/Boss kind."));
                }
            }

            HashSet<string> seenProfiles = new HashSet<string>(StringComparer.Ordinal);
            for (int profileIndex = 0;
                profileIndex < slot.MechanicProfileIds.Count;
                profileIndex++)
            {
                string profileId = slot.MechanicProfileIds[profileIndex];
                string profilePath =
                    path + ".mechanicProfileIds[" + profileIndex + "]";
                ValidateId(profileId, profilePath, issues);
                if (!seenProfiles.Add(profileId))
                {
                    issues.Add(Issue(
                        "MECHANIC_PROFILE_ID_DUPLICATE",
                        profilePath,
                        "A Slot cannot repeat the same exact MechanicProfile ID."));
                }

                if (resolver == null || !kindDefined)
                {
                    continue;
                }

                if (!resolver.TryGetMechanicProfileKind(
                    profileId,
                    out ValidationProfileKind profileKind))
                {
                    issues.Add(Issue(
                        "MECHANIC_PROFILE_REFERENCE_UNRESOLVED",
                        profilePath,
                        "MechanicProfile ID does not resolve with ordinal semantics."));
                    continue;
                }

                ValidationProfileKind expectedKind =
                    slot.Kind == EncounterSlotKind.Enemy
                        ? ValidationProfileKind.Enemy
                        : ValidationProfileKind.Boss;
                if (profileKind != expectedKind)
                {
                    issues.Add(Issue(
                        "MECHANIC_PROFILE_KIND_MISMATCH",
                        profilePath,
                        "MechanicProfile kind must match the Slot carrier kind."));
                }

                if (carrierResolved
                    && !resolver.HasCarrierMechanicBinding(
                        slot.Kind,
                        slot.CarrierId,
                        profileId))
                {
                    issues.Add(Issue(
                        "CARRIER_MECHANIC_BINDING_MISSING",
                        profilePath,
                        "Carrier-to-MechanicProfile binding is absent from E03."));
                }
            }

            if (resolver != null
                && kindDefined
                && slot.MechanicProfileIds.Count == 0
                && !resolver.IsIntentionalMechaniclessCarrier(
                    slot.Kind,
                    slot.CarrierId))
            {
                issues.Add(Issue(
                    "MECHANIC_PROFILE_REQUIRED",
                    path + ".mechanicProfileIds",
                    "Empty MechanicProfile selection requires an E03 intentional carrier exception."));
            }
        }

        private static void ValidateContiguousOrder(
            IEnumerable<int> values,
            string path,
            string code,
            string message,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            int[] ordered = (values ?? Array.Empty<int>())
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            bool contiguous = ordered.Length == 0
                || ordered.Select((value, index) => value == index).All(value => value);
            if (!contiguous)
            {
                issues.Add(Issue(code, path, message));
            }
        }

        private static void ValidateId(
            string id,
            string path,
            ICollection<EncounterCompositionValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(Issue(
                    "ID_EMPTY",
                    path,
                    "Stable IDs must not be empty or whitespace."));
                return;
            }

            if (!string.Equals(id, id.Trim(), StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "ID_OUTER_WHITESPACE",
                    path,
                    "Stable IDs are never trimmed implicitly."));
            }
        }

        private static EncounterCompositionValidationIssue Issue(
            string code,
            string path,
            string message)
        {
            return new EncounterCompositionValidationIssue(code, path, message);
        }
    }
}
