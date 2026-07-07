using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleContractDiagnostics
    {
        public bool devOnly = true;
        public bool isEnabled;
        public int leakCount;
        public List<string> validationWarnings = new();
        public List<string> developerOnlyDiagnostics = new();
        public List<string> sourceDataPaths = new();

        public bool Passed => leakCount == 0 && validationWarnings.Count == 0;

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                validationWarnings.Add(warning.Trim());
            }
        }

        public void AddDeveloperDiagnostic(string diagnostic)
        {
            if (!string.IsNullOrWhiteSpace(diagnostic))
            {
                developerOnlyDiagnostics.Add(diagnostic.Trim());
            }
        }

        public void AddSourceDataPath(string sourceDataPath)
        {
            if (!string.IsNullOrWhiteSpace(sourceDataPath))
            {
                sourceDataPaths.Add(sourceDataPath.Trim());
            }
        }
    }
}
