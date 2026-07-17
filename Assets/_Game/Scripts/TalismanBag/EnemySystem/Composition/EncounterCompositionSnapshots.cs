using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Contracts;

namespace TalismanBag.EnemySystem.Composition
{
    public sealed class EncounterSlotSnapshot
    {
        private readonly ReadOnlyCollection<string> mechanicProfileIds;

        public EncounterSlotSnapshot(
            string slotId,
            int spawnOrder,
            EncounterSlotKind kind,
            EncounterSlotRole role,
            string carrierId,
            int quantity,
            IReadOnlyList<string> mechanicProfileIds)
        {
            SlotId = EncounterCompositionReadOnly.Text(slotId);
            SpawnOrder = spawnOrder;
            Kind = kind;
            Role = role;
            CarrierId = EncounterCompositionReadOnly.Text(carrierId);
            Quantity = quantity;
            this.mechanicProfileIds = EncounterCompositionReadOnly.Strings(mechanicProfileIds, true);
        }

        public string SlotId { get; }
        public int SpawnOrder { get; }
        public EncounterSlotKind Kind { get; }
        public EncounterSlotRole Role { get; }
        public string CarrierId { get; }
        public int Quantity { get; }
        public IReadOnlyList<string> MechanicProfileIds => mechanicProfileIds;

        internal EncounterSlotSnapshot Clone()
        {
            return new EncounterSlotSnapshot(
                SlotId,
                SpawnOrder,
                Kind,
                Role,
                CarrierId,
                Quantity,
                mechanicProfileIds);
        }
    }

    public sealed class EncounterWaveSnapshot
    {
        private readonly ReadOnlyCollection<EncounterSlotSnapshot> slots;

        public EncounterWaveSnapshot(
            string waveId,
            int waveOrder,
            IReadOnlyList<EncounterSlotSnapshot> slots)
        {
            WaveId = EncounterCompositionReadOnly.Text(waveId);
            WaveOrder = waveOrder;
            this.slots = EncounterCompositionReadOnly.Freeze(slots, value => value.Clone())
                .OrderBy(value => value == null ? int.MaxValue : value.SpawnOrder)
                .ThenBy(value => value == null ? string.Empty : value.SlotId, StringComparer.Ordinal)
                .ToReadOnly();
        }

        public string WaveId { get; }
        public int WaveOrder { get; }
        public IReadOnlyList<EncounterSlotSnapshot> Slots => slots;

        public bool TryGetSlotById(string slotId, out EncounterSlotSnapshot slot)
        {
            if (slotId != null)
            {
                foreach (EncounterSlotSnapshot candidate in slots)
                {
                    if (candidate != null
                        && string.Equals(candidate.SlotId, slotId, StringComparison.Ordinal))
                    {
                        slot = candidate;
                        return true;
                    }
                }
            }

            slot = null;
            return false;
        }

        internal EncounterWaveSnapshot Clone()
        {
            return new EncounterWaveSnapshot(WaveId, WaveOrder, slots);
        }
    }

    public sealed class EncounterCompositionSnapshot
    {
        private readonly ReadOnlyCollection<string> mapRuleIds;
        private readonly ReadOnlyCollection<EncounterWaveSnapshot> waves;
        private readonly ReadOnlyCollection<string> developerContentTagIds;

        public EncounterCompositionSnapshot(
            EncounterReference encounterReference,
            IReadOnlyList<string> mapRuleIds,
            IReadOnlyList<EncounterWaveSnapshot> waves,
            IReadOnlyList<string> developerContentTagIds)
        {
            EncounterReference = encounterReference == null
                ? null
                : new EncounterReference(
                    encounterReference.StableId,
                    encounterReference.DevOnly,
                    encounterReference.IsEnabled,
                    encounterReference.EntersFormalFlow);
            this.mapRuleIds = EncounterCompositionReadOnly.Strings(mapRuleIds, true);
            this.waves = EncounterCompositionReadOnly.Freeze(waves, value => value.Clone())
                .OrderBy(value => value == null ? int.MaxValue : value.WaveOrder)
                .ThenBy(value => value == null ? string.Empty : value.WaveId, StringComparer.Ordinal)
                .ToReadOnly();
            this.developerContentTagIds = EncounterCompositionReadOnly.Strings(developerContentTagIds, true);
        }

        public EncounterReference EncounterReference { get; }
        public string EncounterId => EncounterReference == null ? string.Empty : EncounterReference.StableId;
        public IReadOnlyList<string> MapRuleIds => mapRuleIds;
        public IReadOnlyList<EncounterWaveSnapshot> Waves => waves;
        public IReadOnlyList<string> DeveloperContentTagIds => developerContentTagIds;

        public bool TryGetWaveById(string waveId, out EncounterWaveSnapshot wave)
        {
            if (waveId != null)
            {
                foreach (EncounterWaveSnapshot candidate in waves)
                {
                    if (candidate != null
                        && string.Equals(candidate.WaveId, waveId, StringComparison.Ordinal))
                    {
                        wave = candidate;
                        return true;
                    }
                }
            }

            wave = null;
            return false;
        }

        internal EncounterCompositionSnapshot Clone()
        {
            return new EncounterCompositionSnapshot(
                EncounterReference,
                mapRuleIds,
                waves,
                developerContentTagIds);
        }
    }

    public sealed class EncounterCompositionCatalogInput
    {
        private readonly ReadOnlyCollection<EncounterCompositionSnapshot> encounters;

        public EncounterCompositionCatalogInput(
            IReadOnlyList<EncounterCompositionSnapshot> encounters,
            string schemaId = EncounterCompositionSchema.SchemaId,
            int schemaVersion = EncounterCompositionSchema.SchemaVersion)
        {
            SchemaId = EncounterCompositionReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            this.encounters = EncounterCompositionReadOnly.Freeze(encounters, value => value.Clone());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EncounterCompositionSnapshot> Encounters => encounters;
    }

    public sealed class EncounterCompositionCatalogSnapshot : IEncounterCompositionLookup
    {
        private readonly ReadOnlyCollection<EncounterCompositionSnapshot> encounters;
        private readonly IReadOnlyDictionary<string, EncounterCompositionSnapshot> encounterById;

        internal EncounterCompositionCatalogSnapshot(EncounterCompositionCatalogInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            encounters = EncounterCompositionReadOnly.Freeze(input.Encounters, value => value.Clone())
                .OrderBy(value => value.EncounterId, StringComparer.Ordinal)
                .ToReadOnly();
            encounterById = new ReadOnlyDictionary<string, EncounterCompositionSnapshot>(
                encounters.ToDictionary(value => value.EncounterId, value => value, StringComparer.Ordinal));
            CanonicalSignature = EncounterCompositionCanonical.Hash(
                EncounterCompositionCanonical.Catalog(SchemaId, SchemaVersion, encounters));
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EncounterCompositionSnapshot> Encounters => encounters;
        public string CanonicalSignature { get; }

        public bool TryGetEncounterById(string encounterId, out EncounterCompositionSnapshot encounter)
        {
            if (encounterId == null)
            {
                encounter = null;
                return false;
            }

            return encounterById.TryGetValue(encounterId, out encounter);
        }

        public bool TryGetWaveById(
            string encounterId,
            string waveId,
            out EncounterWaveSnapshot wave)
        {
            if (TryGetEncounterById(encounterId, out EncounterCompositionSnapshot encounter))
            {
                return encounter.TryGetWaveById(waveId, out wave);
            }

            wave = null;
            return false;
        }

        public bool TryGetSlotById(
            string encounterId,
            string waveId,
            string slotId,
            out EncounterSlotSnapshot slot)
        {
            if (TryGetWaveById(encounterId, waveId, out EncounterWaveSnapshot wave))
            {
                return wave.TryGetSlotById(slotId, out slot);
            }

            slot = null;
            return false;
        }
    }

    internal static class EncounterCompositionReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<string> Strings(IEnumerable<string> values, bool sortOrdinal)
        {
            IEnumerable<string> source = (values ?? Array.Empty<string>()).Select(Text);
            if (sortOrdinal)
            {
                source = source.OrderBy(value => value, StringComparer.Ordinal);
            }

            return Array.AsReadOnly(source.ToArray());
        }

        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values, Func<T, T> clone)
            where T : class
        {
            if (clone == null)
            {
                throw new ArgumentNullException(nameof(clone));
            }

            return Array.AsReadOnly((values ?? Array.Empty<T>())
                .Select(value => value == null ? null : clone(value))
                .ToArray());
        }

        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    internal static class EncounterCompositionCanonical
    {
        public static string Catalog(
            string schemaId,
            int schemaVersion,
            IEnumerable<EncounterCompositionSnapshot> encounters)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "encounters", encounters, Encounter);
            return builder.ToString();
        }

        private static string Encounter(EncounterCompositionSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "encounterId", value.EncounterId);
            Field(builder, "devOnly", value.EncounterReference.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", value.EncounterReference.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", value.EncounterReference.EntersFormalFlow ? "1" : "0");
            Strings(builder, "mapRuleIds", value.MapRuleIds);
            OrderedRows(builder, "waves", value.Waves, wave => wave.WaveOrder, Wave);
            Strings(builder, "developerContentTagIds", value.DeveloperContentTagIds);
            return builder.ToString();
        }

        private static string Wave(EncounterWaveSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "waveId", value.WaveId);
            Field(builder, "waveOrder", value.WaveOrder.ToString(CultureInfo.InvariantCulture));
            OrderedRows(builder, "slots", value.Slots, slot => slot.SpawnOrder, Slot);
            return builder.ToString();
        }

        private static string Slot(EncounterSlotSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "slotId", value.SlotId);
            Field(builder, "spawnOrder", value.SpawnOrder.ToString(CultureInfo.InvariantCulture));
            Field(builder, "kind", ((int)value.Kind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "role", ((int)value.Role).ToString(CultureInfo.InvariantCulture));
            Field(builder, "carrierId", value.CarrierId);
            Field(builder, "quantity", value.Quantity.ToString(CultureInfo.InvariantCulture));
            Strings(builder, "mechanicProfileIds", value.MechanicProfileIds);
            return builder.ToString();
        }

        private static void Rows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(value => canonical(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void OrderedRows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, int> order,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .OrderBy(order)
                .ThenBy(canonical, StringComparer.Ordinal)
                .Select(canonical)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void Strings(StringBuilder builder, string name, IEnumerable<string> values)
        {
            string[] rows = (values ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void WriteRows(StringBuilder builder, string name, IReadOnlyList<string> rows)
        {
            Field(builder, name + ".count", rows.Count.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Count; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeValue)
                .Append(';');
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in bytes)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
