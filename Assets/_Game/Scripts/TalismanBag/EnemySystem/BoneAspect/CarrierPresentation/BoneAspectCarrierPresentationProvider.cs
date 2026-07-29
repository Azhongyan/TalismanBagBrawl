using System;
using System.Collections.Generic;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.CarrierPresentation
{
    public sealed class BoneAspectCarrierPresentationCatalogProvider
    {
        public static readonly BoneAspectCarrierPresentationCatalogProvider Instance =
            new BoneAspectCarrierPresentationCatalogProvider();

        private readonly BoneAspectCarrierPresentationCatalogValidator validator;

        public BoneAspectCarrierPresentationCatalogProvider(
            BoneAspectCarrierPresentationCatalogValidator validator = null)
        {
            this.validator = validator ?? BoneAspectCarrierPresentationCatalogValidator.Instance;
        }

        public BoneAspectCarrierPresentationCatalogSnapshot CreateSnapshot(
            BoneAspectCarrierPresentationCatalogInput input,
            EnemyMechanicVocabularySnapshot enemyVocabulary)
        {
            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> issues =
                validator.Validate(input, enemyVocabulary);
            if (issues.Count > 0)
            {
                throw new BoneAspectCarrierPresentationValidationException(issues);
            }

            return new BoneAspectCarrierPresentationCatalogSnapshot(
                input,
                enemyVocabulary.BuildCanonicalSignature());
        }

        public BoneAspectCarrierPresentationCatalogSnapshot CreateDefaultSnapshot()
        {
            EnemyMechanicVocabularySnapshot vocabulary =
                DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput());
            return CreateSnapshot(BoneAspectCarrierPresentationCatalog.CreateInput(), vocabulary);
        }
    }
}
