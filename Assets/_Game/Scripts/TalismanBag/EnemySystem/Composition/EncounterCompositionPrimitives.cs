using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;

namespace TalismanBag.EnemySystem.Composition
{
    public static class EncounterCompositionSchema
    {
        public const string SchemaId = "EncounterComposition.v1";
        public const int SchemaVersion = 1;
    }

    public enum EncounterSlotKind
    {
        Enemy = 0,
        Boss = 1
    }

    public enum EncounterSlotRole
    {
        Normal = 0,
        Elite = 1,
        Boss = 2
    }

    public interface IEncounterCompositionReferenceResolver
    {
        bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy);
        bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss);
        bool TryGetMapRule(string stableId, out MapRuleReference mapRule);
        bool TryGetMechanicProfileKind(string stableId, out ValidationProfileKind kind);
        bool HasCarrierMechanicBinding(EncounterSlotKind kind, string carrierId, string mechanicProfileId);
        bool IsIntentionalMechaniclessCarrier(EncounterSlotKind kind, string carrierId);
    }

    public interface IEncounterCompositionLookup
    {
        bool TryGetEncounterById(string encounterId, out EncounterCompositionSnapshot encounter);
        bool TryGetWaveById(
            string encounterId,
            string waveId,
            out EncounterWaveSnapshot wave);
        bool TryGetSlotById(
            string encounterId,
            string waveId,
            string slotId,
            out EncounterSlotSnapshot slot);
    }

    public interface IEncounterCompositionProvider
    {
        EncounterCompositionCatalogSnapshot CreateSnapshot(
            EncounterCompositionCatalogInput input,
            IEncounterCompositionReferenceResolver resolver);
    }
}
