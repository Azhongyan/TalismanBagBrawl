using System.Collections.Generic;
using TalismanBag.Items.Generation.DropCore;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.DropSandbox
{
    public static class DeterministicItemDropRandom
    {
        public const string AlgorithmId = ItemDropDeterministicDomain.AlgorithmId;
        public const int SupportedGenerationVersion =
            ItemDropDeterministicDomain.SupportedGenerationVersion;

        public static bool TryComputeDomainSeed(
            long rootSeed,
            int generationVersion,
            string dropRequestId,
            string sourceContextId,
            string sourceKey,
            int stageNumber,
            IReadOnlyList<string> domainSegments,
            out ulong domainSeed)
        {
            return ItemDropDeterministicDomain.TryComputeDomainSeed(
                rootSeed,
                generationVersion,
                dropRequestId,
                sourceContextId,
                sourceKey,
                stageNumber,
                domainSegments,
                out domainSeed);
        }

        public static bool TryCreateStream(
            long rootSeed,
            int generationVersion,
            string dropRequestId,
            string sourceContextId,
            string sourceKey,
            int stageNumber,
            IReadOnlyList<string> domainSegments,
            out DeterministicItemRandom.Stream stream)
        {
            return ItemDropDeterministicDomain.TryCreateStream(
                rootSeed,
                generationVersion,
                dropRequestId,
                sourceContextId,
                sourceKey,
                stageNumber,
                domainSegments,
                out stream);
        }
    }
}
