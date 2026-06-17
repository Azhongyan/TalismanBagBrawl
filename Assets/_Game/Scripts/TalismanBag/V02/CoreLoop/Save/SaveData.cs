using System;
using TalismanBag.V02.CoreLoop.Inventory;
using TalismanBag.V02.CoreLoop.Upgrades;

namespace TalismanBag.V02.CoreLoop.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int saveVersion = 1;
        public PlayerResourceData resourceData = new();
        public PlayerItemInventoryData itemInventoryData = new();
        public PlayerTalismanProgressData talismanProgressData = new();
        public MainTrialProgressData mainTrialProgressData = new();

        public void Normalize()
        {
            if (saveVersion <= 0)
            {
                saveVersion = 1;
            }

            resourceData ??= new PlayerResourceData();
            resourceData.Normalize();
            itemInventoryData ??= new PlayerItemInventoryData();
            itemInventoryData.Normalize();
            talismanProgressData ??= new PlayerTalismanProgressData();
            talismanProgressData.Normalize();
            mainTrialProgressData ??= new MainTrialProgressData();
            mainTrialProgressData.Normalize();
        }
    }
}
