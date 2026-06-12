using System;
using UnityEngine;

namespace TalismanBag.Combat
{
    [Serializable]
    public sealed class CombatStats
    {
        public int maxHP = 100;
        public int hp = 100;
        public int shield;
        public int maxMana = 100;
        public int mana;
        public int attackDamage = 8;
        public float attackInterval = 2.5f;
        public int poisonStacks;
        public int burnStacks;

        public void ResetPlayer()
        {
            maxHP = 100;
            hp = 100;
            shield = 0;
            maxMana = 100;
            mana = 0;
            poisonStacks = 0;
            burnStacks = 0;
        }

        public void ResetGhost()
        {
            maxHP = 80;
            hp = 80;
            shield = 0;
            mana = 0;
            maxMana = 0;
            attackDamage = 8;
            attackInterval = 2.5f;
            poisonStacks = 0;
            burnStacks = 0;
        }

        public void AddMana(int amount)
        {
            mana = Mathf.Clamp(mana + amount, 0, maxMana);
        }

        public void Heal(int amount)
        {
            hp = Mathf.Clamp(hp + amount, 0, maxHP);
        }

        public void AddShield(int amount, int shieldCap)
        {
            shield = Mathf.Clamp(shield + amount, 0, shieldCap);
        }

        public int TakeDamage(int amount)
        {
            int blocked = Mathf.Min(shield, amount);
            shield -= blocked;
            int remaining = amount - blocked;
            hp = Mathf.Max(0, hp - remaining);
            return remaining;
        }
    }
}
