using System;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public struct BattleGridCell : IEquatable<BattleGridCell>
    {
        public int x;
        public int y;

        public BattleGridCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(BattleGridCell other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleGridCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public override string ToString()
        {
            return $"{x},{y}";
        }
    }
}
