using System;
using TalismanBag.V02.CoreLoop.Resources;

namespace TalismanBag.V02.CoreLoop.Save
{
    [Serializable]
    public sealed class PlayerResourceData
    {
        public int spiritStone;
        public int talismanPaper;
        public int cinnabar;
        public int basicTalismanEmbryo;
        public int cultivation;

        public int GetAmount(ResourceType type)
        {
            return type switch
            {
                ResourceType.SpiritStone => spiritStone,
                ResourceType.TalismanPaper => talismanPaper,
                ResourceType.Cinnabar => cinnabar,
                ResourceType.BasicTalismanEmbryo => basicTalismanEmbryo,
                ResourceType.Cultivation => cultivation,
                _ => 0
            };
        }

        public void SetAmount(ResourceType type, int amount)
        {
            int safeAmount = Math.Max(0, amount);
            switch (type)
            {
                case ResourceType.SpiritStone:
                    spiritStone = safeAmount;
                    break;
                case ResourceType.TalismanPaper:
                    talismanPaper = safeAmount;
                    break;
                case ResourceType.Cinnabar:
                    cinnabar = safeAmount;
                    break;
                case ResourceType.BasicTalismanEmbryo:
                    basicTalismanEmbryo = safeAmount;
                    break;
                case ResourceType.Cultivation:
                    cultivation = safeAmount;
                    break;
            }
        }

        public int Add(ResourceType type, int amount)
        {
            if (amount <= 0)
            {
                return GetAmount(type);
            }

            long total = (long)GetAmount(type) + amount;
            int safeTotal = total > int.MaxValue ? int.MaxValue : (int)total;
            SetAmount(type, safeTotal);
            return GetAmount(type);
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            if (amount == 0)
            {
                return true;
            }

            int current = GetAmount(type);
            if (current < amount)
            {
                return false;
            }

            SetAmount(type, current - amount);
            return true;
        }

        public void Normalize()
        {
            spiritStone = Math.Max(0, spiritStone);
            talismanPaper = Math.Max(0, talismanPaper);
            cinnabar = Math.Max(0, cinnabar);
            basicTalismanEmbryo = Math.Max(0, basicTalismanEmbryo);
            cultivation = Math.Max(0, cultivation);
        }
    }
}
