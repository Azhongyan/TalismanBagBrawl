using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.V04.WorldMap;

namespace TalismanBag.Editor.V04.WorldMap
{
    public static class V04CampaignStageCompletionRepositoryTests
    {
        public static void RunAll()
        {
            NewStorageLoadsEmptyWithoutWriting();
            SequentialMarksSurviveRepositoryReconstruction();
            DuplicateIsIdempotentAndSkipIsRejected();
            InvalidPayloadsFailSafelyToEmpty();
            ValidUnorderedPrefixNormalizesToCanonicalOrder();
            ResetDeletesOnlyThePackageKey();
            PublicSurfaceContainsOnlyExplicitOperations();
        }

        private static void NewStorageLoadsEmptyWithoutWriting()
        {
            MemoryStorage storage = new MemoryStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);

            V04CampaignStageCompletionSnapshot snapshot = repository.Load();

            AssertEqual(
                "CAMPAIGN_NORMAL_LV1",
                V04CampaignStageCompletionRepository.ProductContext,
                "product context");
            AssertEqual(
                "bone_aspect_chapter_1",
                snapshot.campaignId,
                "campaign identity");
            AssertEqual(0, snapshot.completedStageIds.Count, "default completed count");
            AssertFalse(repository.IsCleared("1-1"), "default 1-1 clear");
            AssertEqual(0, storage.SetCount, "Load write count");
            AssertEqual(0, storage.DeleteCount, "Load delete count");
        }

        private static void SequentialMarksSurviveRepositoryReconstruction()
        {
            MemoryStorage storage = new MemoryStorage();
            V04CampaignStageCompletionRepository writer =
                new V04CampaignStageCompletionRepository(storage);

            AssertTrue(writer.MarkCleared("1-1"), "mark 1-1");
            AssertTrue(writer.MarkCleared("1-2"), "mark 1-2");
            AssertTrue(writer.MarkCleared("1-3"), "mark 1-3");
            AssertTrue(writer.MarkCleared("1-4"), "mark 1-4");
            AssertEqual(4, storage.SetCount, "sequential write count");

            V04CampaignStageCompletionRepository reader =
                new V04CampaignStageCompletionRepository(storage);
            V04CampaignStageCompletionSnapshot loaded = reader.Load();

            AssertSequence(
                new[] { "1-1", "1-2", "1-3", "1-4" },
                loaded.completedStageIds,
                "reloaded completed stages");
            AssertTrue(reader.IsCleared("1-1"), "reloaded 1-1");
            AssertTrue(reader.IsCleared("1-4"), "reloaded 1-4");
            AssertTrue(loaded.IsCleared("1-4"), "snapshot 1-4");
            AssertFalse(reader.IsCleared("1-5"), "1-5 before accepted mark");
            AssertEqual(4, storage.SetCount, "Load must not write");

            AssertTrue(reader.MarkCleared("1-5"), "mark 1-5");
            AssertTrue(reader.IsCleared("1-5"), "1-5 after accepted mark");
            AssertEqual(5, storage.SetCount, "1-5 write count");
        }

        private static void DuplicateIsIdempotentAndSkipIsRejected()
        {
            MemoryStorage storage = new MemoryStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);

            AssertFalse(repository.MarkCleared("1-2"), "skip 1-1");
            AssertFalse(repository.MarkCleared("9-9"), "unknown stage");
            AssertEqual(0, storage.SetCount, "rejected write count");

            AssertTrue(repository.MarkCleared("1-1"), "first 1-1");
            AssertEqual(1, storage.SetCount, "first 1-1 write count");
            AssertTrue(repository.MarkCleared("1-1"), "duplicate 1-1");
            AssertEqual(1, storage.SetCount, "duplicate must not rewrite");

            AssertFalse(repository.MarkCleared("1-3"), "skip 1-2");
            AssertEqual(1, storage.SetCount, "skip must not rewrite");
            AssertSequence(new[] { "1-1" }, repository.Snapshot.completedStageIds, "state after rejects");
        }

        private static void InvalidPayloadsFailSafelyToEmpty()
        {
            string[] invalidPayloads =
            {
                "not-json",
                Payload(2, new[] { "1-1" }),
                Payload(1, new[] { "1-1", "unknown" }),
                Payload(1, new[] { "1-1", "1-3" }),
                Payload(1, new[] { "1-1", "1-1" }),
                "{\"schemaId\":\"V04CampaignStageCompletion.v1\",\"version\":1,"
                    + "\"campaignId\":\"wrong_campaign\",\"completedStageIds\":[\"1-1\"]}",
                "{\"schemaId\":\"V04CampaignStageCompletion.v1\",\"version\":1,"
                    + "\"campaignId\":\"bone_aspect_chapter_1\",\"completedStageIds\":[\"1-1\"],"
                    + "\"unexpected\":true}"
            };

            foreach (string payload in invalidPayloads)
            {
                MemoryStorage storage = new MemoryStorage();
                storage.Seed(V04CampaignStageCompletionRepository.StorageKey, payload);
                V04CampaignStageCompletionRepository repository =
                    new V04CampaignStageCompletionRepository(storage);

                V04CampaignStageCompletionSnapshot snapshot = repository.Load();

                AssertEqual(0, snapshot.completedStageIds.Count, "invalid payload completed count");
                AssertFalse(repository.IsCleared("1-1"), "invalid payload 1-1 clear");
                AssertEqual(0, storage.SetCount, "invalid payload must not rewrite");
                AssertEqual(0, storage.DeleteCount, "invalid payload must not delete");
            }
        }

        private static void ValidUnorderedPrefixNormalizesToCanonicalOrder()
        {
            MemoryStorage storage = new MemoryStorage();
            storage.Seed(
                V04CampaignStageCompletionRepository.StorageKey,
                Payload(1, new[] { " 1-3 ", "1-1", "1-2" }));
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);

            V04CampaignStageCompletionSnapshot snapshot = repository.Load();

            AssertSequence(
                new[] { "1-1", "1-2", "1-3" },
                snapshot.completedStageIds,
                "normalized prefix order");
            AssertEqual(0, storage.SetCount, "normalization must not rewrite");
        }

        private static void ResetDeletesOnlyThePackageKey()
        {
            const string OtherKey = "unrelated.package.key";
            MemoryStorage storage = new MemoryStorage();
            storage.Seed(OtherKey, "preserve-me");
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            AssertTrue(repository.MarkCleared("1-1"), "mark before reset");

            repository.Reset();

            AssertFalse(
                storage.HasKey(V04CampaignStageCompletionRepository.StorageKey),
                "package key after reset");
            AssertTrue(storage.HasKey(OtherKey), "unrelated key after reset");
            AssertEqual("preserve-me", storage.GetString(OtherKey), "unrelated value after reset");
            AssertEqual(1, storage.DeleteCount, "reset delete count");
            AssertEqual(
                V04CampaignStageCompletionRepository.StorageKey,
                storage.DeletedKeys.Single(),
                "reset deleted key");
            AssertEqual(0, repository.Snapshot.completedStageIds.Count, "reset snapshot count");
        }

        private static void PublicSurfaceContainsOnlyExplicitOperations()
        {
            string[] methodNames = typeof(V04CampaignStageCompletionRepository)
                .GetMethods(
                    System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            AssertSequence(
                new[] { "IsCleared", "Load", "MarkCleared", "Reset", "get_Snapshot" },
                methodNames,
                "repository public operations");
            AssertEqual(
                typeof(object),
                typeof(V04CampaignStageCompletionRepository).BaseType,
                "repository base type");
        }

        private static string Payload(int version, IReadOnlyList<string> completedStageIds)
        {
            return "{\"schemaId\":\"V04CampaignStageCompletion.v1\",\"version\":"
                + version
                + ",\"campaignId\":\"bone_aspect_chapter_1\",\"completedStageIds\":["
                + string.Join(",", completedStageIds.Select(value => "\"" + value + "\""))
                + "]}";
        }

        private static void AssertTrue(bool value, string label)
        {
            if (!value)
            {
                throw new InvalidOperationException(label + " expected true");
            }
        }

        private static void AssertFalse(bool value, string label)
        {
            if (value)
            {
                throw new InvalidOperationException(label + " expected false");
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string label)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    label + " expected=" + expected + " actual=" + actual);
            }
        }

        private static void AssertSequence(
            IReadOnlyList<string> expected,
            IReadOnlyList<string> actual,
            string label)
        {
            if (expected.Count != actual.Count)
            {
                throw new InvalidOperationException(
                    label + " expectedCount=" + expected.Count + " actualCount=" + actual.Count);
            }

            for (int index = 0; index < expected.Count; index++)
            {
                if (!string.Equals(expected[index], actual[index], StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        label + " index=" + index + " expected=" + expected[index]
                        + " actual=" + actual[index]);
                }
            }
        }

        private sealed class MemoryStorage : IV04CampaignStageCompletionStorage
        {
            private readonly Dictionary<string, string> values =
                new Dictionary<string, string>(StringComparer.Ordinal);

            public int SetCount { get; private set; }

            public int DeleteCount { get; private set; }

            public List<string> DeletedKeys { get; } = new List<string>();

            public bool HasKey(string key)
            {
                return values.ContainsKey(key);
            }

            public string GetString(string key)
            {
                return values.TryGetValue(key, out string value) ? value : string.Empty;
            }

            public void SetString(string key, string value)
            {
                SetCount++;
                values[key] = value;
            }

            public void DeleteKey(string key)
            {
                DeleteCount++;
                DeletedKeys.Add(key);
                values.Remove(key);
            }

            public void Seed(string key, string value)
            {
                values[key] = value;
            }
        }
    }
}
