using TalismanBag.Items;
using TalismanBag.Items.Capability;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection
{
    public sealed class DefaultLayoutResilienceItemFactProjectionAdapter :
        ILayoutResilienceItemFactProjectionAdapter
    {
        public static readonly DefaultLayoutResilienceItemFactProjectionAdapter Instance =
            new DefaultLayoutResilienceItemFactProjectionAdapter();

        private readonly DefaultLayoutResilienceItemFactProjectionValidator validator;

        public DefaultLayoutResilienceItemFactProjectionAdapter(
            DefaultLayoutResilienceItemFactProjectionValidator validator = null)
        {
            this.validator = validator ??
                DefaultLayoutResilienceItemFactProjectionValidator.Instance;
        }

        public LayoutResilienceItemFactProjectionResult Project(
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot)
        {
            ProjectionAssessment assessment = validator.Assess(
                itemSnapshot, bindingSnapshot);
            LayoutResilienceInputCompleteness completeness =
                assessment.Status == LayoutResilienceItemFactProjectionStatus.Complete
                    ? LayoutResilienceInputCompleteness.Complete
                    : LayoutResilienceInputCompleteness.Incomplete;
            string signature = LayoutResilienceItemFactProjectionCanonical.Signature(
                itemSnapshot,
                bindingSnapshot,
                assessment.Status,
                completeness,
                assessment.BuildFacts,
                assessment.Issues);
            LayoutResilienceItemFactProjectionResult result =
                new LayoutResilienceItemFactProjectionResult(
                    assessment.Status,
                    completeness,
                    assessment.BuildFacts,
                    assessment.Issues,
                    signature);
            if (validator.ValidateResult(result).Count != 0)
            {
                throw new System.InvalidOperationException(
                    "Internal item fact projection result validation failed.");
            }
            return result;
        }
    }
}
