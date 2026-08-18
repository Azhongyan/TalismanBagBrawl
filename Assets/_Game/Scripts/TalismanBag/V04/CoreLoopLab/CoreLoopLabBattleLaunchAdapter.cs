using System;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.DevSession;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using UnityEngine;

namespace TalismanBag.V04.CoreLoopLab
{
    public sealed class CoreLoopLabBattleLaunchAdapter
    {
        private readonly BuildGridInteractionPreviewController gridController;
        private readonly CoreLoopLabProfile labProfile;
        private readonly string hostSessionToken;
        private readonly int resetGeneration;
        private readonly BattleSandboxDevBattleSessionEngine engine = new();

        public CoreLoopLabBattleLaunchAdapter(
            BuildGridInteractionPreviewController grid,
            CoreLoopLabProfile profile,
            string sessionToken,
            int generation)
        {
            gridController = grid;
            labProfile = profile;
            hostSessionToken = sessionToken ?? string.Empty;
            resetGeneration = generation;
        }

        public BattleSandboxDevBattleSessionSnapshot Current => engine.Current;
        public bool IsRunning => engine.IsRunning;

        public bool TryAcceptAuthoredPrepareLaunch(
            int battleIndex,
            int encounterGeneration,
            string selectedRewardBaseItemId,
            CoreLoopLabEncounterProfile profile,
            out BattleSandboxDevBattleSessionAcceptedStart result,
            out string diagnosticCode)
        {
            result = null;
            diagnosticCode = "CORE_LOOP_LAB_LAUNCH_NOT_ATTEMPTED";
            if (battleIndex <= 0 || encounterGeneration <= 0 || profile == null
                || gridController == null || string.IsNullOrWhiteSpace(
                    hostSessionToken) || resetGeneration <= 0)
            {
                diagnosticCode = "CORE_LOOP_LAB_LAUNCH_DEPENDENCY_MISSING";
                return false;
            }
            if (!gridController.IsSandboxBattleModeActive)
            {
                diagnosticCode = "CORE_LOOP_LAB_AUTHORED_PREPARE_NOT_READY";
                return false;
            }

            string encounterToken = hostSessionToken + "|battle="
                + battleIndex.ToString(CultureInfo.InvariantCulture)
                + "|encounter=" + encounterGeneration.ToString(
                    CultureInfo.InvariantCulture);
            BattleSandboxExplicitDevEncounterRequest encounter =
                profile.BuildRequest(encounterGeneration, encounterToken);
            IItemSystemBattleSandboxBoardAuthority authority =
                ResolveAuthority(out diagnosticCode);
            if (authority == null)
            {
                return false;
            }
            if (!BattleSandboxDevBattleSessionAssembler.TryAssemble(
                    authority,
                    hostSessionToken,
                    resetGeneration,
                    CoreLoopLabFixedContent.ContractProfileId(labProfile),
                    CoreLoopLabFixedContent.MechanicsProfileId(labProfile),
                    battleIndex,
                    selectedRewardBaseItemId,
                    encounter,
                    out BattleSandboxDevBattleSessionRequest request,
                    out diagnosticCode))
            {
                return false;
            }

            if (engine.Current?.IsTerminal == true
                && engine.Current.BattleIndex != battleIndex)
            {
                engine.Reset();
            }
            if (!engine.TryStart(request, Application.isEditor, out result))
            {
                diagnosticCode = engine.LastDiagnosticCode;
                return false;
            }

            diagnosticCode = "NONE";
            return true;
        }

        public bool TryAdvance(
            long deltaMilliseconds,
            out BattleSandboxDevBattleSessionSnapshot snapshot,
            out string diagnosticCode)
        {
            snapshot = engine.Current;
            IItemSystemBattleSandboxBoardAuthority authority =
                ResolveAuthority(out diagnosticCode);
            if (authority == null)
            {
                return false;
            }
            if (!BattleSandboxDevBattleSessionAssembler
                    .TryCaptureLiveSignatures(
                        authority,
                        resetGeneration,
                        out BattleSandboxDevBattleLiveSignatures signatures,
                        out diagnosticCode))
            {
                return false;
            }
            if (!engine.TryAdvanceBy(
                    Math.Max(0L, deltaMilliseconds),
                    signatures,
                    out snapshot))
            {
                diagnosticCode = engine.LastDiagnosticCode;
                return false;
            }
            diagnosticCode = "NONE";
            return true;
        }

        public void Reset()
        {
            engine.Reset();
        }

        private IItemSystemBattleSandboxBoardAuthority ResolveAuthority(
            out string diagnosticCode)
        {
            diagnosticCode = "NONE";
            if (gridController == null)
            {
                diagnosticCode = "ITEM_AUTHORITY_CONTROLLER_MISSING";
                return null;
            }

            ItemSystemBattleSandboxBoardAdapter[] adapters =
                gridController.gameObject.scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ItemSystemBattleSandboxBoardAdapter>(true))
                    .Where(value => value != null
                        && value.gameObject.scene ==
                            gridController.gameObject.scene)
                    .ToArray();
            if (adapters.Length != 1 || adapters[0].Authority == null)
            {
                diagnosticCode =
                    "ITEM_AUTHORITY_EXACT_SCENE_BINDING_REQUIRED";
                return null;
            }

            return adapters[0].Authority;
        }
    }
}
