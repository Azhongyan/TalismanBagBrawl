using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience
{
    public static class LayoutResilienceStructuralPredicateSchema
    {
        public const string SchemaId = "LayoutResilienceStructuralPredicate.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResiliencePredicateState
    {
        KnownTrue = 1,
        KnownFalse = 2,
        Unknown = 3,
        NotApplicable = 4
    }

    public enum LayoutResilienceInputCompleteness
    {
        Complete = 1,
        Incomplete = 2,
        NotRequired = 3
    }

    public enum LayoutPressureKind
    {
        PollutedCellMask = 1,
        EyeRelocationOrDisruption = 2,
        StructuralConnectionCut = 3
    }

    public enum LayoutResiliencePredicateClauseKind
    {
        CountedLayoutPresent = 1,
        CountedPlacementCellsUsable = 2,
        CountedPlacementCoresUsable = 3,
        EffectiveEyeAnchorUsable = 4,
        EyeToCountedCoreStructurallyConnected = 5
    }

    public enum LayoutResiliencePredicateClauseState
    {
        Satisfied = 1,
        Violated = 2
    }

    public sealed class LayoutCellCoordinate : IEquatable<LayoutCellCoordinate>
    {
        public LayoutCellCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(LayoutCellCoordinate other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LayoutCellCoordinate);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + ":" +
                Y.ToString(CultureInfo.InvariantCulture);
        }

        internal LayoutCellCoordinate Clone()
        {
            return new LayoutCellCoordinate(X, Y);
        }
    }

    public sealed class LayoutCellConnection
    {
        public LayoutCellConnection(LayoutCellCoordinate cellA, LayoutCellCoordinate cellB)
        {
            CellA = cellA == null ? null : cellA.Clone();
            CellB = cellB == null ? null : cellB.Clone();
        }

        public LayoutCellCoordinate CellA { get; }
        public LayoutCellCoordinate CellB { get; }

        internal LayoutCellConnection Clone()
        {
            return new LayoutCellConnection(CellA, CellB);
        }
    }

    public interface ILayoutResilienceStructuralPredicateValidator
    {
        IReadOnlyList<LayoutResilienceValidationIssue> Validate(
            LayoutResilienceEvaluationInput input);

        IReadOnlyList<LayoutResilienceValidationIssue> ValidateResult(
            LayoutResilienceEvaluationInput input,
            LayoutResiliencePredicateResultSnapshot result);
    }

    public interface ILayoutResilienceStructuralPredicateEvaluator
    {
        LayoutResiliencePredicateResultSnapshot Evaluate(
            LayoutResilienceEvaluationInput input);
    }

    internal static class LayoutResilienceReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }

        public static ReadOnlyCollection<LayoutCellCoordinate> FreezeCells(
            IEnumerable<LayoutCellCoordinate> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<LayoutCellCoordinate>())
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value, LayoutCellCoordinateComparer.Instance)
                .ToArray());
        }

        public static ReadOnlyCollection<LayoutCellConnection> FreezeConnections(
            IEnumerable<LayoutCellConnection> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<LayoutCellConnection>())
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value, LayoutCellConnectionComparer.Instance)
                .ToArray());
        }
    }

    internal sealed class LayoutCellCoordinateComparer :
        IComparer<LayoutCellCoordinate>
    {
        public static readonly LayoutCellCoordinateComparer Instance =
            new LayoutCellCoordinateComparer();

        public int Compare(LayoutCellCoordinate left, LayoutCellCoordinate right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left == null)
            {
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            int y = left.Y.CompareTo(right.Y);
            return y != 0 ? y : left.X.CompareTo(right.X);
        }
    }

    internal sealed class LayoutCellConnectionComparer :
        IComparer<LayoutCellConnection>
    {
        public static readonly LayoutCellConnectionComparer Instance =
            new LayoutCellConnectionComparer();

        public int Compare(LayoutCellConnection left, LayoutCellConnection right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left == null)
            {
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            int first = LayoutCellCoordinateComparer.Instance.Compare(left.CellA, right.CellA);
            return first != 0
                ? first
                : LayoutCellCoordinateComparer.Instance.Compare(left.CellB, right.CellB);
        }
    }
}
