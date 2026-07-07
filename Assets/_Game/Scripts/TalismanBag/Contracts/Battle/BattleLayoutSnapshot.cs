using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleLayoutSnapshot
    {
        public string snapshotId = string.Empty;
        public string boardId = string.Empty;
        public int gridWidth;
        public int gridHeight;
        public List<BattleItemSnapshot> placedItems = new();
        public List<BattleFormationEyeSnapshot> formationEyes = new();
        public List<BattleEnergySourceSnapshot> energySources = new();
        public List<BattleGridCell> blockedCells = new();
        public List<BattleGridCell> lockedCells = new();
        public BattleContractEnergyState currentEnergyState = BattleContractEnergyState.Unknown;
        public int createdAtFrame;
        public bool devOnly = true;

        public string sourceAdapter = string.Empty;
        public bool legacySingleCell;
        public List<string> energyDiagnostics = new();
        public List<string> validationWarnings = new();
    }

    [Serializable]
    public sealed class BattleFormationEyeSnapshot
    {
        public string eyeId = string.Empty;
        public BattleGridCell cell;
        public BattleContractEnergyState energyState = BattleContractEnergyState.Unknown;
        public int stability;
        public bool devOnly = true;
    }

    [Serializable]
    public sealed class BattleEnergySourceSnapshot
    {
        public string energySourceId = string.Empty;
        public string itemId = string.Empty;
        public BattleGridCell cell;
        public BattleContractEnergyState energyState = BattleContractEnergyState.Unknown;
        public List<string> suppliedItemIds = new();
        public bool devOnly = true;
    }
}
