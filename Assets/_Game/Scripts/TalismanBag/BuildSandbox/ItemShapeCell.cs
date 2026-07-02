using System;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public struct ItemShapeCell : IEquatable<ItemShapeCell>
    {
        public int x;
        public int y;

        public ItemShapeCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(ItemShapeCell other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemShapeCell other && Equals(other);
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
