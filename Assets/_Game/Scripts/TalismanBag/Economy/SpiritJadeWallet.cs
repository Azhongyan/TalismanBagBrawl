using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Economy
{
    public sealed class SpiritJadeWallet : MonoBehaviour
    {
        [SerializeField] private Text jadeText;

        public int CurrentJade { get; private set; }
        public event Action<int> JadeChanged;

        public void ResetWallet(int startingAmount)
        {
            CurrentJade = Mathf.Max(0, startingAmount);
            Refresh();
        }

        public void AddJade(int amount)
        {
            CurrentJade = Mathf.Max(0, CurrentJade + amount);
            Refresh();
        }

        public bool CanSpend(int amount)
        {
            return CurrentJade >= amount;
        }

        public bool SpendJade(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            CurrentJade -= amount;
            Refresh();
            return true;
        }

        private void Refresh()
        {
            if (jadeText != null)
            {
                jadeText.text = $"灵玉 {CurrentJade}";
            }

            JadeChanged?.Invoke(CurrentJade);
        }
    }
}
