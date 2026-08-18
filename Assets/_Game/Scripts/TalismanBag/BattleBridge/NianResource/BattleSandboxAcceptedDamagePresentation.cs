using System.Collections.Generic;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.Combat;

namespace TalismanBag.BattleBridge.NianResource
{
    public interface IBattleSandboxAcceptedDamagePresentation
    {
        string EventId { get; }
        int ResetGeneration { get; }
        long BattleTick { get; }
        string SourceRequestId { get; }
        string SourceBaseItemId { get; }
        string SourceItemInstanceId { get; }
        string SourcePlacementId { get; }
        IReadOnlyList<ItemShapeCell> OccupiedCells { get; }
        int ResolvedPreMitigationDamageUnits { get; }
        int ShellDamageApplied { get; }
        int HpDamageApplied { get; }
        int TotalDamageApplied { get; }
        bool ShellBreakApplied { get; }
        bool IsAcceptedCorrelation { get; }
    }

    public interface IBattleSandboxAcceptedDamagePresentationSource
    {
        string PresentationSourceId { get; }
        bool HasActivePresentationSession { get; }
        int PresentationGeneration { get; }
        IBattleSandboxAcceptedDamagePresentation
            LastAcceptedDamagePresentation { get; }
        ItemCombatEffectRequestSnapshot
            CurrentPresentationItemRequestSnapshot { get; }
    }
}
