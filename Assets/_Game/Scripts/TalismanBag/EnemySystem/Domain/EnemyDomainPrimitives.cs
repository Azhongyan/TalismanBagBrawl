using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.Domain
{
    public static class EnemyDomainSchema
    {
        public const string SchemaId = "EnemyDomainContract.v1";
        public const int SchemaVersion = 1;
    }

    public enum EnemyArchetypeCategory
    {
        Normal = 0,
        Elite = 1,
        Boss = 2
    }

    public interface IEnemyDomainIsolationMetadata
    {
        bool DevOnly { get; }
        bool IsEnabled { get; }
        bool EntersFormalFlow { get; }
    }

    internal static class EnemyDomainReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<string> Ids(IEnumerable<string> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<string>())
                .Select(Text)
                .ToArray());
        }

        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values, Func<T, T> clone)
        {
            if (clone == null)
            {
                throw new ArgumentNullException(nameof(clone));
            }

            return Array.AsReadOnly((values ?? Array.Empty<T>())
                .Select(value => value is null ? default : clone(value))
                .ToArray());
        }
    }
}
