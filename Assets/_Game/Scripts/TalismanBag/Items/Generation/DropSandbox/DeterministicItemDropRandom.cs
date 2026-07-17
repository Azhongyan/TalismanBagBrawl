using System;
using System.Collections.Generic;
using System.Text;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.DropSandbox
{
    public static class DeterministicItemDropRandom
    {
        public const string AlgorithmId = "item-drop-generation-v1";
        public const int SupportedGenerationVersion = 1;

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
            domainSeed = 0UL;
            if (generationVersion != SupportedGenerationVersion
                || string.IsNullOrWhiteSpace(dropRequestId)
                || string.IsNullOrWhiteSpace(sourceContextId)
                || string.IsNullOrWhiteSpace(sourceKey)
                || stageNumber <= 0
                || domainSegments == null
                || domainSegments.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < domainSegments.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(domainSegments[index]))
                {
                    return false;
                }
            }

            ulong hash = DeterministicItemRandom.Fnv1A64OffsetBasis;
            AppendString(ref hash, AlgorithmId);
            AppendUInt64(ref hash, unchecked((ulong)rootSeed));
            AppendUInt32(ref hash, unchecked((uint)generationVersion));
            AppendString(ref hash, dropRequestId.Trim());
            AppendString(ref hash, sourceContextId.Trim());
            AppendString(ref hash, sourceKey.Trim());
            AppendUInt32(ref hash, unchecked((uint)stageNumber));
            AppendUInt32(ref hash, unchecked((uint)domainSegments.Count));
            for (int index = 0; index < domainSegments.Count; index++)
            {
                AppendString(ref hash, domainSegments[index]);
            }

            domainSeed = hash;
            return true;
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
            stream = null;
            if (!TryComputeDomainSeed(rootSeed, generationVersion, dropRequestId, sourceContextId,
                sourceKey, stageNumber, domainSegments, out ulong seed))
            {
                return false;
            }

            stream = new DeterministicItemRandom.Stream(seed);
            return true;
        }

        private static void AppendString(ref ulong hash, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            AppendUInt32(ref hash, unchecked((uint)bytes.Length));
            for (int index = 0; index < bytes.Length; index++)
            {
                AppendByte(ref hash, bytes[index]);
            }
        }

        private static void AppendUInt32(ref ulong hash, uint value)
        {
            AppendByte(ref hash, (byte)value);
            AppendByte(ref hash, (byte)(value >> 8));
            AppendByte(ref hash, (byte)(value >> 16));
            AppendByte(ref hash, (byte)(value >> 24));
        }

        private static void AppendUInt64(ref ulong hash, ulong value)
        {
            for (int shift = 0; shift < 64; shift += 8)
            {
                AppendByte(ref hash, (byte)(value >> shift));
            }
        }

        private static void AppendByte(ref ulong hash, byte value)
        {
            hash = unchecked((hash ^ value) * DeterministicItemRandom.Fnv1A64Prime);
        }
    }
}
