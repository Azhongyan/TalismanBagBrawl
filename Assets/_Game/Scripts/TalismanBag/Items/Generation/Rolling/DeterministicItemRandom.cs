using System;
using System.Collections.Generic;
using System.Text;

namespace TalismanBag.Items.Generation.Rolling
{
    public static class DeterministicItemRandom
    {
        public const string AlgorithmId = "item-instance-roll-v1";
        public const int SupportedGenerationVersion = 1;
        public const ulong Fnv1A64OffsetBasis = 14695981039346656037UL;
        public const ulong Fnv1A64Prime = 1099511628211UL;
        private const ulong SplitMixIncrement = 0x9E3779B97F4A7C15UL;

        public static ulong Fnv1A64Utf8(string value)
        {
            ulong hash = Fnv1A64OffsetBasis;
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            for (int index = 0; index < bytes.Length; index++)
            {
                hash = unchecked((hash ^ bytes[index]) * Fnv1A64Prime);
            }

            return hash;
        }

        public static bool TryComputeDomainSeed(
            long rootSeed,
            int generationVersion,
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            IReadOnlyList<string> domainSegments,
            out ulong domainSeed)
        {
            domainSeed = 0UL;
            if (generationVersion != SupportedGenerationVersion
                || string.IsNullOrWhiteSpace(itemInstanceId)
                || string.IsNullOrWhiteSpace(baseItemId)
                || !ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _)
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

            ulong hash = Fnv1A64OffsetBasis;
            AppendString(ref hash, AlgorithmId);
            AppendUInt64(ref hash, unchecked((ulong)rootSeed));
            AppendUInt32(ref hash, unchecked((uint)generationVersion));
            AppendString(ref hash, itemInstanceId.Trim());
            AppendString(ref hash, baseItemId.Trim());
            AppendString(ref hash, rarity.ToStableKey());
            AppendUInt32(ref hash, unchecked((uint)domainSegments.Count));
            for (int index = 0; index < domainSegments.Count; index++)
            {
                AppendString(ref hash, domainSegments[index]);
            }

            domainSeed = hash;
            return true;
        }

        public static ulong SplitMix64(ulong state)
        {
            unchecked
            {
                ulong value = state + SplitMixIncrement;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }

        public sealed class Stream
        {
            private ulong state;

            public Stream(ulong seed)
            {
                state = seed;
            }

            public ulong NextUInt64()
            {
                unchecked
                {
                    state += SplitMixIncrement;
                    ulong value = state;
                    value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                    value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                    return value ^ (value >> 31);
                }
            }

            public bool TryNextBounded(ulong exclusiveUpperBound, out ulong value)
            {
                value = 0UL;
                if (exclusiveUpperBound == 0UL)
                {
                    return false;
                }

                ulong rejectionThreshold = unchecked(0UL - exclusiveUpperBound) % exclusiveUpperBound;
                ulong sample;
                do
                {
                    sample = NextUInt64();
                }
                while (sample < rejectionThreshold);

                value = sample % exclusiveUpperBound;
                return true;
            }
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
            hash = unchecked((hash ^ value) * Fnv1A64Prime);
        }
    }
}
