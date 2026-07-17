using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SystemSnapshot
{
    public sealed class EnemySystemSnapshotInput
    {
        public EnemySystemSnapshotInput(
            EnemyDomainSnapshot enemyDomainSnapshot,
            EnemyMechanicVocabularySnapshot enemyMechanicVocabularySnapshot,
            EnemyValidationContentSnapshot enemyValidationContentSnapshot,
            EncounterCompositionCatalogSnapshot encounterCompositionCatalogSnapshot,
            EnemySkillBossPhaseCatalogSnapshot enemySkillBossPhaseCatalogSnapshot,
            CounterWindowAndPressureCatalogSnapshot counterWindowAndPressureCatalogSnapshot)
        {
            EnemyDomainSnapshot = enemyDomainSnapshot;
            EnemyMechanicVocabularySnapshot = enemyMechanicVocabularySnapshot;
            EnemyValidationContentSnapshot = enemyValidationContentSnapshot;
            EncounterCompositionCatalogSnapshot = encounterCompositionCatalogSnapshot;
            EnemySkillBossPhaseCatalogSnapshot = enemySkillBossPhaseCatalogSnapshot;
            CounterWindowAndPressureCatalogSnapshot = counterWindowAndPressureCatalogSnapshot;
        }

        public EnemyDomainSnapshot EnemyDomainSnapshot { get; }
        public EnemyMechanicVocabularySnapshot EnemyMechanicVocabularySnapshot { get; }
        public EnemyValidationContentSnapshot EnemyValidationContentSnapshot { get; }
        public EncounterCompositionCatalogSnapshot EncounterCompositionCatalogSnapshot { get; }
        public EnemySkillBossPhaseCatalogSnapshot EnemySkillBossPhaseCatalogSnapshot { get; }
        public CounterWindowAndPressureCatalogSnapshot CounterWindowAndPressureCatalogSnapshot { get; }
    }
}
