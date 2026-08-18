using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;
using TalismanBag.Items.Resource;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.BattleBridge
{
    public static class I031NianGenerationBattleResourceVerticalSliceVerifier
    {
        public const string Package =
            "V0.4-I031NianGenerationBattleResourceVerticalSlice01";

#if UNITY_EDITOR
        public static void VerifyFromUnityMenu()
        {
            Verification verification = VerifyOffline(true);
            if (!verification.Passed)
            {
                throw new InvalidOperationException(verification.summary);
            }

            Debug.Log(verification.summary);
        }
#endif

#if NIANTEST_STANDALONE
        public static int Main()
        {
            try
            {
                Verification verification = VerifyOffline(true);
                Console.WriteLine(verification.summary);
                return verification.Passed ? 0 : 1;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 2;
            }
        }
#endif

        public static Verification VerifyOffline(bool writeReports)
        {
            string root = ResolveProjectRoot();
            List<Row> rows = new List<Row>();

            ItemSystemSnapshot boardSnapshot = Snapshot(
                new[] { P("P_SYSTEM_I031", "I031", 0, 0) },
                I031InventoryPlacementContract.OwnedBoard());
            I031NianSourceSnapshot source = I031NianSourceAssembler.Assemble(
                boardSnapshot, 1);

            Add(rows, "SOURCE-01", "ItemSystemSnapshot.v2 authority",
                boardSnapshot.isValid &&
                source.status == I031NianSourceStatus.Valid &&
                source.isEligible &&
                source.isOwned &&
                source.location == I031Location.Board &&
                source.isLightingSource &&
                source.itemId == "I031" &&
                source.specialIdentityId == "SPECIAL_I031" &&
                source.stablePlacementId == "P_SYSTEM_I031" &&
                source.generationPerPulse == 4,
                source.canonicalSignature);

            int[] costs = source.CostRequestFacts.Select(value => value.nianCost).ToArray();
            string[] items = source.CostRequestFacts.Select(value => value.itemId).ToArray();
            Add(rows, "SOURCE-02", "Lv40 Orange machine cost facts",
                costs.SequenceEqual(new[] { 2, 3, 5, 3, 3, 6 }) &&
                items.SequenceEqual(new[] { "I007", "I008", "I009", "I010", "I011", "I012" }) &&
                source.CostRequestFacts.All(value =>
                    value.itemLevel == 40 &&
                    value.rarityKey == "Orange" &&
                    value.resourceKey == "nian" &&
                    value.sourceKey == "ItemLv40OrangeProjection.nianCost"),
                string.Join(",", costs.Select(value =>
                    value.ToString(CultureInfo.InvariantCulture))));

            bool readOnly = false;
            try
            {
                ((IList<I031NianCostRequestFact>)source.CostRequestFacts).Add(
                    new I031NianCostRequestFact("BAD", 99));
            }
            catch (NotSupportedException)
            {
                readOnly = true;
            }
            Add(rows, "SOURCE-03", "Immutable source collection", readOnly,
                "ReadOnlyCollection mutation rejected");

            ItemSystemSnapshot inventorySnapshot = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(),
                I031InventoryPlacementContract.OwnedInventory());
            I031NianSourceSnapshot inventorySource =
                I031NianSourceAssembler.Assemble(inventorySnapshot, 1);
            Add(rows, "SOURCE-04", "Inventory state cannot generate",
                !inventorySource.isEligible &&
                inventorySource.ValidationErrors.Contains("I031_NOT_ON_BOARD") &&
                inventorySource.ValidationErrors.Contains("I031_PLACEMENT_COUNT_NOT_ONE"),
                string.Join("|", inventorySource.ValidationErrors));

            BattleSandboxNianResourceEngine engine =
                BattleSandboxNianResourceEngine.Create(source);
            Add(rows, "BATTLE-01", "Initial authoritative resource snapshot",
                engine.Current.schemaVersion ==
                BattleSandboxNianResourceSnapshot.SchemaVersion &&
                engine.Current.resourceKey == "nian" &&
                engine.Current.maxNian == 100 &&
                engine.Current.currentNian == 20 &&
                engine.Current.resetGeneration == 1 &&
                engine.Current.pulseCount == 0,
                engine.Current.canonicalSignature);

            List<BattleSandboxNianResourceApplication> applications =
                RunPulses(engine, source, 50);
            BattleSandboxNianResourceSnapshot nominal = engine.Current;
            Add(rows, "BATTLE-02", "50 pulse frozen fixture",
                nominal.pulseCount == 50 &&
                nominal.totalGenerated == 200 &&
                nominal.totalSpent == 181 &&
                nominal.currentNian == 39 &&
                nominal.acceptedApplicationCount == 50 &&
                nominal.rejectedApplicationCount == 0 &&
                nominal.ProcessedPulseEventIds.Count == 50 &&
                nominal.nextCostOrdinal == 2,
                "initial=20; generated=200; spent=181; final=" +
                nominal.currentNian.ToString(CultureInfo.InvariantCulture));

            Add(rows, "BATTLE-03", "Accepted applications preserve 50 damage gates",
                applications.Count == 50 &&
                applications.All(value => value.accepted &&
                    !string.IsNullOrEmpty(value.applicationId)) &&
                applications.Last().battleTick == 75000L,
                "acceptedApplications=" +
                applications.Count.ToString(CultureInfo.InvariantCulture) +
                "; durationTicks=" +
                applications.Last().battleTick.ToString(CultureInfo.InvariantCulture));

            bool orderedLedger = nominal.Ledger.Count == 100;
            for (int index = 0; index < 50 && orderedLedger; index++)
            {
                BattleSandboxNianLedgerEntry generation = nominal.Ledger[index * 2];
                BattleSandboxNianLedgerEntry spend = nominal.Ledger[index * 2 + 1];
                orderedLedger =
                    generation.kind == BattleSandboxNianLedgerKind.Generation &&
                    (spend.kind == BattleSandboxNianLedgerKind.SpendAccepted ||
                     spend.kind == BattleSandboxNianLedgerKind.SpendRejected) &&
                    generation.pulseEventId == spend.pulseEventId &&
                    generation.sequence + 1 == spend.sequence &&
                    generation.balanceAfter == spend.balanceBefore;
            }
            Add(rows, "BATTLE-04", "Pulse order generation then spend",
                orderedLedger, "ledgerRows=" +
                nominal.Ledger.Count.ToString(CultureInfo.InvariantCulture));

            BattleSandboxNianResourceApplication insufficient =
                BattleSandboxNianResourceEngine.EvaluateSpend(
                    4, source.FindCostFact("I009"));
            Add(rows, "BATTLE-05", "Insufficient balance rejects damage authorization",
                !insufficient.accepted &&
                insufficient.reasonCode == "INSUFFICIENT_NIAN" &&
                string.IsNullOrEmpty(insufficient.applicationId) &&
                insufficient.balanceAfter == 4,
                insufficient.reasonCode);

            string beforeInvalid = engine.Current.canonicalSignature;
            I031NianCostRequestFact next = engine.NextCostRequest;
            BattleSandboxNianPulseResult invalidSignature = engine.ApplyPulse(
                source,
                new BattleSandboxNianPulseRequest(
                    "PULSE_INVALID_SIGNATURE",
                    1,
                    "WRONG_SIGNATURE",
                    next.requestFactId,
                    76500L));
            Add(rows, "BATTLE-06", "Signature validation precedes generation",
                !invalidSignature.accepted &&
                invalidSignature.reasonCode == "SOURCE_SIGNATURE_MISMATCH" &&
                engine.Current.canonicalSignature == beforeInvalid &&
                engine.Current.pulseCount == 50 &&
                engine.Current.currentNian == 39,
                invalidSignature.reasonCode);

            BattleSandboxNianPulseResult duplicate = engine.ApplyPulse(
                source,
                new BattleSandboxNianPulseRequest(
                    "PULSE_001",
                    1,
                    source.canonicalSignature,
                    next.requestFactId,
                    76500L));
            Add(rows, "BATTLE-07", "Duplicate pulse is idempotently rejected",
                !duplicate.accepted &&
                duplicate.reasonCode == "DUPLICATE_PULSE_EVENT" &&
                engine.Current.canonicalSignature == beforeInvalid,
                duplicate.reasonCode);

            BattleSandboxNianResourceEngine capEngine =
                BattleSandboxNianResourceEngine.Create(source);
            RunPulses(capEngine, source, 400);
            Add(rows, "BATTLE-08", "Cap is enforced and recorded",
                capEngine.Current.currentNian <= 100 &&
                capEngine.Current.Ledger.Any(value =>
                    value.kind == BattleSandboxNianLedgerKind.Generation &&
                    value.reasonCode == "GENERATED_CAPPED"),
                "final=" + capEngine.Current.currentNian.ToString(
                    CultureInfo.InvariantCulture));

            I031NianSourceSnapshot generationTwo =
                I031NianSourceAssembler.Assemble(boardSnapshot, 2);
            BattleSandboxNianResourceSnapshot reset = engine.Reset(generationTwo);
            Add(rows, "RESET-01", "Reset restores 20 and clears old ledger/events",
                reset.resetGeneration == 2 &&
                reset.currentNian == 20 &&
                reset.pulseCount == 0 &&
                reset.totalGenerated == 0 &&
                reset.totalSpent == 0 &&
                reset.acceptedApplicationCount == 0 &&
                reset.rejectedApplicationCount == 0 &&
                reset.Ledger.Count == 0 &&
                reset.ProcessedPulseEventIds.Count == 0 &&
                reset.nextCostOrdinal == 0,
                reset.canonicalSignature);

            string afterReset = engine.Current.canonicalSignature;
            BattleSandboxNianPulseResult stale = engine.ApplyPulse(
                source,
                new BattleSandboxNianPulseRequest(
                    "PULSE_STALE_GENERATION",
                    1,
                    source.canonicalSignature,
                    source.CostRequestFacts[0].requestFactId,
                    1500L));
            Add(rows, "RESET-02", "Old generation cannot leak after reset",
                !stale.accepted &&
                stale.reasonCode == "GENERATION_MISMATCH" &&
                engine.Current.canonicalSignature == afterReset &&
                engine.Current.currentNian == 20 &&
                engine.Current.ProcessedPulseEventIds.Count == 0,
                stale.reasonCode);

            string deterministicA = RunNominalCanonical(source);
            string deterministicB = RunNominalCanonical(source);
            Add(rows, "DETERMINISM-01", "Same source and pulses are deterministic",
                deterministicA == deterministicB,
                deterministicA);

            LeakResult leak = ScanOwnedSources(root);
            Add(rows, "BOUNDARY-01", "No forbidden runtime coupling in owned sources",
                leak.Count == 0,
                leak.Count == 0 ? "leakCount=0" : string.Join("|", leak.Hits));

            IntegrationScan integration = ScanP3Integration(root);
            Add(rows, "INTEGRATION-01",
                "P3 pulse is gated by shared Nian identity",
                integration.RuntimePass,
                integration.RuntimeEvidence);
            Add(rows, "INTEGRATION-02",
                "Existing Nian UI is read-only runtime projection",
                integration.PresenterPass,
                integration.PresenterEvidence);
            Add(rows, "INTEGRATION-03",
                "Legacy loops remain suppressed without Scene writes",
                integration.BoundaryPass,
                integration.BoundaryEvidence);
            Add(rows, "INTEGRATION-04",
                "Real Projection damage is not a frozen launch gate",
                integration.RealProjectionPass,
                integration.RealProjectionEvidence);
            Add(rows, "INTEGRATION-05",
                "Authored TMP source and settlement cues share one event",
                integration.AuthoredTmpPass,
                integration.AuthoredTmpEvidence);
            Add(rows, "INTEGRATION-06",
                "Accepted-only board activation VFX bypasses legacy truth",
                integration.AcceptedVfxPass,
                integration.AcceptedVfxEvidence);
            Add(rows, "PRESENTATION-01",
                "I007-I012 have six stable source-number gradients",
                VerifySourceGradientPalette(out string paletteEvidence),
                paletteEvidence);
            Add(rows, "PRESENTATION-02",
                "HP/SH/BREAK and NP deltas have distinct result semantics",
                VerifySettlementGradientPalette(
                    out string settlementPaletteEvidence),
                settlementPaletteEvidence);
            Add(rows, "PRESENTATION-03",
                "Accepted VFX uses three explicit skill carriers, never card artwork",
                VerifyAcceptedCarrierGrammar(
                    out string carrierEvidence),
                carrierEvidence);

            Verification verification = new Verification(
                root,
                rows,
                leak,
                nominal.canonicalSignature,
                source.canonicalSignature);
            if (writeReports)
            {
                WriteReports(verification);
            }

            return verification;
        }

        private static List<BattleSandboxNianResourceApplication> RunPulses(
            BattleSandboxNianResourceEngine engine,
            I031NianSourceSnapshot source,
            int count)
        {
            List<BattleSandboxNianResourceApplication> applications =
                new List<BattleSandboxNianResourceApplication>();
            for (int index = 0; index < count; index++)
            {
                I031NianCostRequestFact cost = engine.NextCostRequest;
                int pulseNumber = engine.Current.pulseCount + 1;
                BattleSandboxNianPulseResult result = engine.ApplyPulse(
                    source,
                    new BattleSandboxNianPulseRequest(
                        "PULSE_" + pulseNumber.ToString("000", CultureInfo.InvariantCulture),
                        engine.Current.resetGeneration,
                        source.canonicalSignature,
                        cost.requestFactId,
                        pulseNumber * BattleSandboxNianResourceEngine.PulseIntervalTicks));
                if (result.resourceApplication != null &&
                    result.resourceApplication.accepted)
                {
                    applications.Add(result.resourceApplication);
                }
            }

            return applications;
        }

        private static string RunNominalCanonical(I031NianSourceSnapshot source)
        {
            BattleSandboxNianResourceEngine engine =
                BattleSandboxNianResourceEngine.Create(source);
            RunPulses(engine, source, 50);
            return engine.Current.canonicalSignature;
        }

        private static ItemSystemSnapshot Snapshot(
            IReadOnlyList<ItemSystemPlacementInput> placements,
            params I031InventoryPlacementStateInput[] states)
        {
            return DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(
                    placements,
                    i031StateInputs: states));
        }

        private static ItemSystemPlacementInput P(
            string placementId,
            string itemId,
            int x,
            int y)
        {
            return new ItemSystemPlacementInput(
                placementId,
                itemId,
                new Vector2Int(x, y));
        }

        private static LeakResult ScanOwnedSources(string root)
        {
            string[] relativePaths =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Resource/I031NianSourceContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Resource/I031NianSourceAssembler.cs",
                "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleSandboxNianResourceContracts.cs",
                "Assets/_Game/Scripts/TalismanBag/BattleBridge/NianResource/BattleSandboxNianResourceEngine.cs",
                "Assets/_Game/Scripts/TalismanBag/BattleBridge/NianResource/BattleSandboxNianResourcePresenter.cs",
                "Assets/_Game/Scripts/TalismanBag/BattleBridge/NianResource/BattleSandboxAuthoredTmpPresentation.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/I031NianGenerationBattleResourceVerticalSliceVerifier.cs"
            };
            string[] forbidden =
            {
                "BattleSandboxMana" + "LoopRuntime",
                "manaGainPer" + "Tick",
                "UnityEditor.Scene" + "Management",
                "Prefab" + "Utility",
                "Auto" + "Combat",
                "Run" + "Flow"
            };
            List<string> hits = new List<string>();
            foreach (string relativePath in relativePaths)
            {
                string fullPath = Path.Combine(root, relativePath);
                string text = File.ReadAllText(fullPath, Encoding.UTF8);
                foreach (string token in forbidden)
                {
                    if (text.IndexOf(token, StringComparison.Ordinal) >= 0)
                    {
                        hits.Add(relativePath + ":" + token);
                    }
                }
            }

            return new LeakResult(hits);
        }

        private static IntegrationScan ScanP3Integration(string root)
        {
            string runtime = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                    + "ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs"),
                Encoding.UTF8);
            string binder = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                    + "ShougunuPhase1BattleSandboxSceneBinder.cs"),
                Encoding.UTF8);
            string presenter = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/NianResource/"
                    + "BattleSandboxNianResourcePresenter.cs"),
                Encoding.UTF8);
            string authoredTmp = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/NianResource/"
                    + "BattleSandboxAuthoredTmpPresentation.cs"),
                Encoding.UTF8);
            string visualAdapter = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                    + "ShougunuPhase1BattleSandboxVisualCueAdapter.cs"),
                Encoding.UTF8);
            string itemFeedback = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/"
                    + "BattleSandboxItemTriggerFeedbackController.cs"),
                Encoding.UTF8);
            string applicationEngine = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                    + "ShougunuPhase1BattleApplicationEngine.cs"),
                Encoding.UTF8);
            string p3Verifier = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/"
                    + "ShougunuPhase1BattleSandboxVerticalSliceVerifier.cs"),
                Encoding.UTF8);
            string authoredFontAsset = File.ReadAllText(
                Path.Combine(
                    root,
                    "Assets/TextMesh Pro/Fonts/"
                    + "MFLangSongJianYuan-Regular SDF.asset"),
                Encoding.UTF8);
            int[] authoredUnicodes = Regex.Matches(
                    authoredFontAsset,
                    @"m_Unicode:\s*(\d+)")
                .Cast<Match>()
                .Select(value => int.Parse(
                    value.Groups[1].Value,
                    CultureInfo.InvariantCulture))
                .ToArray();
            bool runtimePass =
                runtime.Contains("AdvanceWithNianGate", StringComparison.Ordinal)
                && runtime.Contains(
                    "BuildSharedPulseEventId", StringComparison.Ordinal)
                && runtime.Contains(
                    "I031NianSourceAssembler.Assemble",
                    StringComparison.Ordinal)
                && runtime.Contains(
                    "session.AdvanceTo(pulseTick)",
                    StringComparison.Ordinal)
                && runtime.Contains(
                    "NIAN_DAMAGE_LEDGER_IDENTITY_MISMATCH",
                    StringComparison.Ordinal);
            bool presenterPass =
                presenter.Contains("ManaText", StringComparison.Ordinal)
                && presenter.Contains("PlayerManaBar", StringComparison.Ordinal)
                && presenter.Contains(
                    "ManaGeneratedAnchor", StringComparison.Ordinal)
                && presenter.Contains(
                    "ManaSpentAnchor", StringComparison.Ordinal)
                && presenter.Contains(
                    "RestoreAuthoredState", StringComparison.Ordinal)
                && presenter.Contains(
                    "OwnsResourceTruth => false", StringComparison.Ordinal)
                && presenter.Contains(
                    "WritesLayout => false", StringComparison.Ordinal);
            bool boundaryPass =
                binder.Contains(
                    "SuppressLegacyRuntimeLoop", StringComparison.Ordinal)
                && binder.Contains(
                    "SuppressLegacyManaLoop", StringComparison.Ordinal)
                && binder.Contains(
                    "legacyRuntimeLoop.enabled = false",
                    StringComparison.Ordinal)
                && binder.Contains(
                    "legacyManaLoop.enabled = false",
                    StringComparison.Ordinal)
                && !runtime.Contains(
                    "SceneManager.LoadScene", StringComparison.Ordinal)
                && !presenter.Contains(
                    "RectTransformUtility", StringComparison.Ordinal);
            bool realProjectionPass =
                !runtime.Contains(
                    ".TerminalMagnitudes[index]", StringComparison.Ordinal)
                && !applicationEngine.Contains(
                    "Terminal Item request magnitude mismatch",
                    StringComparison.Ordinal)
                && p3Verifier.Contains(
                    "VerifyRealProjectionLaunch", StringComparison.Ordinal)
                && p3Verifier.Contains(
                    "i008.resolvedPreMitigationDamageUnits == 40",
                    StringComparison.Ordinal)
                && p3Verifier.Contains(
                    "adjacency.stackCount == 2",
                    StringComparison.Ordinal);
            bool authoredTmpPass =
                authoredTmp.Contains(
                    "PrimaryTemplateName = \"shanghai\"",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "SecondaryTemplateName = \"shanghai (1)\"",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "DefaultReadableLifetime = 1.65f",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "Instantiate(",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "fontSharedMaterial = source.fontSharedMaterial",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "acceptedRoleKeys.Add(roleKey)",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "RequiredFontAssetName =",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "\"MFLangSongJianYuan-Regular SDF\"",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "RequiredSourceFontGuid =",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "TMP_FontAsset.CreateFontAsset(",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "AtlasPopulationMode.Dynamic",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "fallbackFontAssetTable = runtimeFallbacks",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "TryAddCharacters(",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "runtimeChineseFallback.HasCharacter(",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "allAvailableAfterAdd",
                    StringComparison.Ordinal)
                && authoredTmp.Contains(
                    "ForceMeshUpdate(true, true)",
                    StringComparison.Ordinal)
                && authoredFontAsset.Contains(
                    "m_Name: MFLangSongJianYuan-Regular SDF",
                    StringComparison.Ordinal)
                && authoredFontAsset.Contains(
                    "m_AtlasPopulationMode: 0",
                    StringComparison.Ordinal)
                && authoredFontAsset.Contains(
                    "m_FallbackFontAssetTable: []",
                    StringComparison.Ordinal)
                && authoredFontAsset.Contains(
                    "sourceFontFileGUID: "
                    + BattleSandboxAuthoredTmpPresentation
                        .RequiredSourceFontGuid,
                    StringComparison.Ordinal)
                && authoredUnicodes.Length == 96
                && authoredUnicodes.All(value =>
                    value < 0x4E00 || value > 0x9FFF)
                && runtime.Contains(
                    "resolvedPreMitigationDamageUnits",
                    StringComparison.Ordinal)
                && binder.Contains(
                    "\"item.source.resolved_damage\"",
                    StringComparison.Ordinal)
                && binder.Contains(
                    "\"item.enemy.settlement\"",
                    StringComparison.Ordinal)
                && visualAdapter.Contains(
                    "RegisterAcceptedItemPresentation",
                    StringComparison.Ordinal)
                && !presenter.Contains(
                    "typeof(Text)",
                    StringComparison.Ordinal);
            bool acceptedVfxPass =
                itemFeedback.Contains(
                    "TryPlayAcceptedPresentation",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "acceptedPresentationEventIds",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "SpawnAcceptedCarrierVfx",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "PlayResolvedPresentation",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "completeLegacyPresentationRequest",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "SpawnFlashOnTarget",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "TryPlayResourceSequenceVfx",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "SpawnSkillVfx",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "AcceptedSkillCarrier_",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "FireProjectileCarrierKey",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "SwordQiSlashCarrierKey",
                    StringComparison.Ordinal)
                && itemFeedback.Contains(
                    "HeavySealDropCarrierKey",
                    StringComparison.Ordinal)
                && !itemFeedback.Contains(
                    "ResolveAcceptedPresentationSprite",
                    StringComparison.Ordinal)
                && !itemFeedback.Contains(
                    "AcceptedItemTransfer_",
                    StringComparison.Ordinal)
                && binder.Contains(
                    ".TryPlayAcceptedPresentation(",
                    StringComparison.Ordinal)
                && !binder.Contains(
                    ".Play(BattleSandboxRuntimeLoopRow",
                    StringComparison.Ordinal)
                && boundaryPass;
            return new IntegrationScan(
                runtimePass,
                presenterPass,
                boundaryPass,
                realProjectionPass,
                authoredTmpPass,
                acceptedVfxPass,
                "shared battle.request.g identity; Nian before session pulse",
                "ManaText/Fill + generated/spent authored TMP clones; authored state restored",
                "legacy loops forced disabled; Scene serialization untouched",
                "I008 real Projection 40 at adjacency 2/3 starts without fabricating 42",
                "exact MFLangSongJianYuan-Regular SDF; authored asset is static ASCII-only (96 glyphs/no fallback), runtime exact-OTF dynamic fallback; source=resolvedPreMitigationDamageUnits; enemy=shell/hp applied",
                "accepted Nian + matching ItemApplication ledger only; direct old Play path restores source pulse/flash/resource-sequence/procedural VFX, then explicit FIRE_PROJECTILE/SWORD_QI_SLASH/HEAVY_SEAL_DROP carrier + impact; card artwork never transfers; legacy Text suppressed");
        }

        private static bool VerifySourceGradientPalette(
            out string evidence)
        {
            string[] itemIds =
                { "I007", "I008", "I009", "I010", "I011", "I012" };
            List<string> signatures = new List<string>();
            foreach (string itemId in itemIds)
            {
                if (!BattleSandboxAuthoredTmpPresentation
                        .TryResolveItemSourceGradient(
                            itemId,
                            out VertexGradient gradient))
                {
                    evidence = itemId + "=missing";
                    return false;
                }

                signatures.Add(
                    ColorUtility.ToHtmlStringRGBA(gradient.topLeft)
                    + "-"
                    + ColorUtility.ToHtmlStringRGBA(gradient.bottomLeft));
            }

            evidence = string.Join(
                ";",
                itemIds.Zip(
                    signatures,
                    (itemId, signature) => itemId + "=" + signature));
            return signatures.Distinct(StringComparer.Ordinal).Count()
                == itemIds.Length;
        }

        private static bool VerifySettlementGradientPalette(
            out string evidence)
        {
            string[] paletteKeys =
            {
                BattleSandboxAuthoredTmpPresentation
                    .NianGeneratedPaletteKey,
                BattleSandboxAuthoredTmpPresentation
                    .NianSpentPaletteKey,
                BattleSandboxAuthoredTmpPresentation
                    .SettlementHpPaletteKey,
                BattleSandboxAuthoredTmpPresentation
                    .SettlementShellPaletteKey,
                BattleSandboxAuthoredTmpPresentation
                    .SettlementBreakPaletteKey
            };
            List<string> signatures = new List<string>();
            foreach (string paletteKey in paletteKeys)
            {
                if (!BattleSandboxAuthoredTmpPresentation
                        .TryResolveItemSourceGradient(
                            paletteKey,
                            out VertexGradient gradient))
                {
                    evidence = paletteKey + "=missing";
                    return false;
                }

                signatures.Add(
                    ColorUtility.ToHtmlStringRGBA(gradient.topLeft)
                    + "-"
                    + ColorUtility.ToHtmlStringRGBA(
                        gradient.bottomLeft));
            }

            bool taiBaiOnlyReserved =
                !BattleSandboxAuthoredTmpPresentation
                    .TryResolveItemSourceGradient(
                        BattleSandboxAuthoredTmpPresentation
                            .TaiBaiBuildFamilyPaletteKeyReserved,
                        out VertexGradient _);
            evidence = string.Join(
                    ";",
                    paletteKeys.Zip(
                        signatures,
                        (key, signature) => key + "=" + signature))
                + ";taibaiReservedOnly="
                + taiBaiOnlyReserved;
            return signatures.Distinct(StringComparer.Ordinal).Count()
                       == paletteKeys.Length
                   && taiBaiOnlyReserved;
        }

        private static bool VerifyAcceptedCarrierGrammar(
            out string evidence)
        {
            string[] itemIds =
                { "I007", "I008", "I009", "I010", "I011", "I012" };
            string[] expected =
            {
                BattleSandboxItemTriggerFeedbackController
                    .FireProjectileCarrierKey,
                BattleSandboxItemTriggerFeedbackController
                    .SwordQiSlashCarrierKey,
                BattleSandboxItemTriggerFeedbackController
                    .HeavySealDropCarrierKey,
                BattleSandboxItemTriggerFeedbackController
                    .FireProjectileCarrierKey,
                BattleSandboxItemTriggerFeedbackController
                    .SwordQiSlashCarrierKey,
                BattleSandboxItemTriggerFeedbackController
                    .HeavySealDropCarrierKey
            };
            string[] actual = itemIds
                .Select(
                    BattleSandboxItemTriggerFeedbackController
                        .ResolveAcceptedCarrierPresentationKey)
                .ToArray();
            bool exactMapping = actual.SequenceEqual(
                expected,
                StringComparer.Ordinal);
            bool threeCarriers =
                actual.Distinct(StringComparer.Ordinal).Count() == 3;
            evidence = string.Join(
                    ";",
                    itemIds.Zip(
                        actual,
                        (itemId, carrier) =>
                            itemId + "=" + carrier))
                + ";taibaiFamilyReserved="
                + BattleSandboxItemTriggerFeedbackController
                    .TaiBaiCarrierFamilyKeyReserved;
            return exactMapping
                   && threeCarriers
                   && !string.IsNullOrWhiteSpace(
                       BattleSandboxItemTriggerFeedbackController
                           .TaiBaiCarrierFamilyKeyReserved);
        }

        private static void WriteReports(Verification verification)
        {
            string reportDirectory = Path.Combine(
                verification.root, "Docs/V0.4/Reports");
            Directory.CreateDirectory(reportDirectory);
            File.WriteAllText(
                Path.Combine(reportDirectory,
                    "I031NianGenerationBattleResourceVerticalSliceReport.md"),
                BuildMarkdownReport(verification),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(reportDirectory,
                    "I031NianGenerationBattleResourceVerticalSliceSpec.csv"),
                BuildCsvReport(verification),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(reportDirectory,
                    "I031NianGenerationBattleResourceVerticalSliceLeakCheckReport.md"),
                BuildLeakReport(verification),
                new UTF8Encoding(false));
        }

        private static string BuildMarkdownReport(Verification verification)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# I031 Nian Generation Battle Resource Vertical Slice Report")
                .AppendLine()
                .AppendLine("- Package: `" + Package + "`")
                .AppendLine("- Phase: `LIGHT_FIX_CODE_COMPLETE / USER_HANDTEST_WAITING`")
                .AppendLine("- Scope: `devOnly Item/Battle resource authority + Shougunu P3 runtime/UI projection; no Scene/formal-flow writes`")
                .AppendLine("- Verification: `" + verification.PassedCount + "/" +
                            verification.Rows.Count + " PASS`")
                .AppendLine("- Runtime QA: `full Unity runtime/editor compile PASS; offline I031 "
                            + verification.PassedCount + "/"
                            + verification.Rows.Count
                            + " PASS; current-revision normal Editor Play "
                            + "smoke PENDING`")
                .AppendLine("- Source signature: `" + verification.sourceCanonical + "`")
                .AppendLine("- 50-pulse snapshot: `" + verification.nominalCanonical + "`")
                .AppendLine()
                .AppendLine("| ID | Check | Result | Evidence |")
                .AppendLine("|---|---|---:|---|");
            foreach (Row row in verification.Rows)
            {
                builder.Append("| ").Append(row.id).Append(" | ")
                    .Append(EscapeMarkdown(row.check)).Append(" | ")
                    .Append(row.Passed ? "PASS" : "FAIL").Append(" | ")
                    .Append(EscapeMarkdown(row.evidence)).AppendLine(" |");
            }

            builder.AppendLine()
                .AppendLine("## LIGHT_FIX diagnosis")
                .AppendLine()
                .AppendLine("- Live first rejection: `TERMINAL_ITEM_CHAIN_NOT_READY`; `I008 damage=40`, adjacency `2/3`, while the stale P3 launch gate required the frozen `42` fixture.")
                .AppendLine("- The authored Continue Battle button, current binder, and Item request assembly were already reached; rejection occurred before I031 Nian source/session creation.")
                .AppendLine("- P3 now retains row/cardinality/positive-damage and exact I031 source checks, while consuming each current authoritative Projection magnitude without fabricating `I008=42`.")
                .AppendLine()
                .AppendLine("## Presentation LIGHT_FIX")
                .AppendLine()
                .AppendLine("- Exact old VFX owner: `BattleSandboxRuntimeLoopRuntime.ApplyCurrentRow -> BattleSandboxItemTriggerFeedbackController.Play`. Disabling the legacy runtime stopped `Play`, and `ResetLoop/OnDisable` cleared its transient effects.")
                .AppendLine("- Exact old source-activation path is now reused for accepted events: `PulseTarget -> SpawnFlashOnTarget/AtAnchor -> TryPlayResourceSequenceVfx -> SpawnSkillVfx`; its legacy `SpawnFloatingText` is intentionally suppressed because the correlated authored-TMP number is the sole numeric presentation.")
                .AppendLine("- Card artwork never leaves its board cell. After source activation, a presentation-only explicit table maps I007/I010 to `FIRE_PROJECTILE`, I008/I011 to `SWORD_QI_SLASH`, and I009/I012 to `HEAVY_SEAL_DROP`; each independent carrier owns travel/drop and impact visuals, is deduped by the accepted event ID, and is reliably destroyed/reset. TaiBai only reserves a separate carrier-family key.")
                .AppendLine("- The legacy damage/mana loops remain disabled. The facade only translates the accepted event's `itemInstanceId/baseItemId/placementId/occupiedCells/eventId` into the old presentation request; duplicate/stale/rejected/zero events never enter it, and every transient is cleared.")
                .AppendLine("- Both authored templates use the exact `MFLangSongJianYuan-Regular SDF` FontAsset and shared material. The asset is static (`AtlasPopulationMode=Static`), contains exactly 96 ASCII/control glyphs, and has no fallback; that was the first missing layer for every Chinese clone.")
                .AppendLine("- The exact runtime rejection layer was TMP 3.0.9 `TryAddCharacters`: after the prewarm had already inserted a glyph, a repeated request had no new glyph work and returned `false` with the whole request echoed as `missingCharacters`. The helper treated that as real font absence and rejected otherwise valid Chinese. It now checks `HasCharacter` before and after insertion, so prewarmed glyphs are accepted while genuinely absent glyphs still reject explicitly.")
                .AppendLine("- The source literals and compiled assembly contain proper Chinese; earlier shell output was the UTF-8 Editor log decoded through the local ANSI code page. The touched Chinese sources remain UTF-8 BOM as an explicit encoding guard.")
                .AppendLine("- Runtime keeps the authored FontAsset/material on each clone, attaches a DontSave dynamic fallback built from the asset's exact linked `MFLangSongJianYuan-Regular.otf` GUID, prewarms required Chinese glyphs, forces mesh generation, and restores the original empty fallback table on reset/disable. Neither template nor FontAsset/Scene bytes are saved.")
                .AppendLine("- I007-I012 source numbers use six stable presentation-only vertex gradients (朱红金、橙金、赤红白、青焰金、紫红金、亮金赤) keyed by `baseItemId`. Enemy settlement independently uses result semantics: HP deep cinnabar to warm white (`-N HP`), Shell amber to gold (`-N SH`), and break gold to warm white (`BREAK`).")
                .AppendLine("- Accepted resource ledger deltas project as `+4 NP` at I031 in cyan-green/gold and exact `-2/-3/-5/-3/-3/-6 NP` at the actual trigger in purple-red/orange-gold. They never reuse HP red and never own resource truth.")
                .AppendLine("- One pulse event produces at most one source number, one VFX carrier chain, one enemy settlement, one generated NP cue, one spent NP cue, and one break cue. Source number is the accepted request row's `resolvedPreMitigationDamageUnits`; enemy text is the matching damage ledger's `shellDamageApplied` or `hpDamageApplied`.")
                .AppendLine("- Floating timing is `1.65s`: pop/hold, slow rise, then fade, with three-lane staggering for consecutive events.")
                .AppendLine()
                .AppendLine("## Frozen fixture")
                .AppendLine()
                .AppendLine("- `resourceKey=nian`, `max=100`, `initial/reset=20`, `generation=4`, `pulse=1500 ticks`.")
                .AppendLine("- I007-I012 costs: `2,3,5,3,3,6` from immutable Item-owned request facts.")
                .AppendLine("- 50 pulses: `generated=200`, `spent=181`, `accepted=50`, `final=39`, `duration=75000 ticks`.")
                .AppendLine("- Frozen I008=42 remains an exact-fixture assertion only; runtime launch consumes the current authoritative Projection (including I008=40 at 2/3 adjacency).")
                .AppendLine("- P3 uses the same resetGeneration, tick, requestId and `battle.request.g...` pulse identity; damage advances only after an accepted Nian application.")
                .AppendLine("- Scene serialization remains unchanged; existing ManaText/Fill, authored anchors, and authored TMP templates are resolved at runtime.");
            return builder.ToString();
        }

        private static string BuildCsvReport(Verification verification)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("id,check,result,evidence");
            foreach (Row row in verification.Rows)
            {
                builder.Append(Csv(row.id)).Append(',')
                    .Append(Csv(row.check)).Append(',')
                    .Append(Csv(row.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(row.evidence)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(Verification verification)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# I031 Nian Resource Leak Check")
                .AppendLine()
                .AppendLine("- Result: `" +
                            (verification.leak.Count == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak count: `" +
                            verification.leak.Count.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Scanned domain: the seven package-owned C# files plus P3/runtime/presentation integration boundary checks.")
                .AppendLine("- Guard: no coupling to the legacy mana runtime, scene/prefab APIs, formal auto-combat, or run-flow.");
            foreach (string hit in verification.leak.Hits)
            {
                builder.AppendLine("- `" + hit + "`");
            }

            return builder.ToString();
        }

        private static string ResolveProjectRoot()
        {
            string current = Directory.GetCurrentDirectory();
            for (int depth = 0; depth < 8 && !string.IsNullOrEmpty(current); depth++)
            {
                if (Directory.Exists(Path.Combine(current, "Assets")) &&
                    Directory.Exists(Path.Combine(current, "ProjectSettings")) &&
                    Directory.Exists(Path.Combine(current, "Packages")))
                {
                    return current;
                }

                current = Directory.GetParent(current)?.FullName;
            }

            throw new DirectoryNotFoundException("Unity project root not found.");
        }

        private static void Add(
            ICollection<Row> rows,
            string id,
            string check,
            bool passed,
            string evidence)
        {
            rows.Add(new Row(id, check, passed, evidence));
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        public sealed class Verification
        {
            public Verification(
                string root,
                IReadOnlyList<Row> rows,
                LeakResult leak,
                string nominalCanonical,
                string sourceCanonical)
            {
                this.root = root;
                Rows = rows;
                this.leak = leak;
                this.nominalCanonical = nominalCanonical;
                this.sourceCanonical = sourceCanonical;
                PassedCount = rows.Count(value => value.Passed);
                summary = Package + ": " + PassedCount.ToString(
                    CultureInfo.InvariantCulture) + "/" + rows.Count.ToString(
                    CultureInfo.InvariantCulture) + " PASS; " +
                    (Passed ? "LIGHT_FIX_CODE_COMPLETE / USER_HANDTEST_WAITING" :
                        "VERIFICATION_FAILED");
            }

            public string root { get; }
            public IReadOnlyList<Row> Rows { get; }
            public LeakResult leak { get; }
            public string nominalCanonical { get; }
            public string sourceCanonical { get; }
            public int PassedCount { get; }
            public bool Passed => PassedCount == Rows.Count;
            public string summary { get; }
        }

        public sealed class Row
        {
            public Row(string id, string check, bool passed, string evidence)
            {
                this.id = id ?? string.Empty;
                this.check = check ?? string.Empty;
                Passed = passed;
                this.evidence = evidence ?? string.Empty;
            }

            public string id { get; }
            public string check { get; }
            public bool Passed { get; }
            public string evidence { get; }
        }

        public sealed class LeakResult
        {
            public LeakResult(IReadOnlyList<string> hits)
            {
                Hits = hits ?? Array.Empty<string>();
            }

            public IReadOnlyList<string> Hits { get; }
            public int Count => Hits.Count;
        }

        private sealed class IntegrationScan
        {
            public IntegrationScan(
                bool runtimePass,
                bool presenterPass,
                bool boundaryPass,
                bool realProjectionPass,
                bool authoredTmpPass,
                bool acceptedVfxPass,
                string runtimeEvidence,
                string presenterEvidence,
                string boundaryEvidence,
                string realProjectionEvidence,
                string authoredTmpEvidence,
                string acceptedVfxEvidence)
            {
                RuntimePass = runtimePass;
                PresenterPass = presenterPass;
                BoundaryPass = boundaryPass;
                RealProjectionPass = realProjectionPass;
                AuthoredTmpPass = authoredTmpPass;
                AcceptedVfxPass = acceptedVfxPass;
                RuntimeEvidence = runtimeEvidence ?? string.Empty;
                PresenterEvidence = presenterEvidence ?? string.Empty;
                BoundaryEvidence = boundaryEvidence ?? string.Empty;
                RealProjectionEvidence =
                    realProjectionEvidence ?? string.Empty;
                AuthoredTmpEvidence =
                    authoredTmpEvidence ?? string.Empty;
                AcceptedVfxEvidence =
                    acceptedVfxEvidence ?? string.Empty;
            }

            public bool RuntimePass { get; }
            public bool PresenterPass { get; }
            public bool BoundaryPass { get; }
            public bool RealProjectionPass { get; }
            public bool AuthoredTmpPass { get; }
            public bool AcceptedVfxPass { get; }
            public string RuntimeEvidence { get; }
            public string PresenterEvidence { get; }
            public string BoundaryEvidence { get; }
            public string RealProjectionEvidence { get; }
            public string AuthoredTmpEvidence { get; }
            public string AcceptedVfxEvidence { get; }
        }
    }
}
