using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleItemSnapshot
    {
        public string itemInstanceId = string.Empty;
        public string itemId = string.Empty;
        public string displayName = string.Empty;
        public string familyId = string.Empty;
        public string baseItemId = string.Empty;
        public string shapeId = string.Empty;
        public int rotationIndex;
        public BattleGridCell anchorCell;
        public List<BattleGridCell> occupiedCells = new();
        public BattleItemStatsSnapshot itemStats = new();
        public List<string> affixes = new();
        public string rarity = string.Empty;
        public BattleContractEnergyState energyState = BattleContractEnergyState.Unknown;
        public string connectedEyeId = string.Empty;
        public string energySourceId = string.Empty;
        public string sourceContainer = string.Empty;
        public List<string> synergyTags = new();
        public bool legacySingleCell;

        public string runtimeId = string.Empty;
        public string sourceDataPath = string.Empty;
        public List<string> fullRolls = new();
        public bool energyRoleViolation;
        public List<string> developerOnlyDiagnostics = new();
        public List<string> validationWarnings = new();
    }
}
