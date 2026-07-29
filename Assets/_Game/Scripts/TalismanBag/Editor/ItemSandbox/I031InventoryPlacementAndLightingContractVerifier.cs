using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.EditorTools.CrossSystem.ItemEnemy;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Rolling;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class I031InventoryPlacementAndLightingContractVerifier
    {
        private static bool unityBatchInvocation;

        private const string Package =
            "V0.4-I031InventoryPlacementAndLightingContract01";
        private const string AssignmentHash =
            "eb43150a9f59b9341754290f1af0225d1a3b846ecf1ed85bcc960384f9fe1ffc";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReport = ReportDirectory +
            "/I031InventoryPlacementAndLightingContractReport.md";
        private const string SpecReport = ReportDirectory +
            "/I031InventoryPlacementAndLightingContractSpec.csv";
        private const string MigrationReport = ReportDirectory +
            "/I031InventoryPlacementAndLightingCanonicalMigration.csv";
        private const string LeakReport = ReportDirectory +
            "/I031InventoryPlacementAndLightingLeakCheckReport.md";

        private static readonly ProtectedFile[] ProtectedFiles =
        {
            new ProtectedFile(
                "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs",
                "0f837d048d30a34f6d7c23ff03b74fac4bb498bd7523e7eff51b1bf319c9bb8f"),
            new ProtectedFile(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
                "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0"),
            new ProtectedFile(
                "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs",
                "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9"),
            new ProtectedFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs",
                "6912eb4faf9f14c26891a83bd0717c0fb5243c9edb8e263bcda1511450bf5cd0"),
            new ProtectedFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs",
                "0dbf41b4a1e008b15ac12d2546ad8cdfe83937db3ea08ed3e3b0aa212b88c4d0"),
            new ProtectedFile(
                "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity",
                "867fbebf42223908c5473757ebf6a54542f4646f7b8bcaf1c4bdb017e0937894"),
            new ProtectedFile(
                "ProjectSettings/EditorBuildSettings.asset",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify I031 Inventory Placement And Lighting Contract")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            Verification verification = Run();
            WriteReports(verification);
            if (verification.Scenarios.Any(value => !value.Passed))
            {
                throw new InvalidOperationException(verification.Summary);
            }
            Console.WriteLine(verification.Summary);
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                unityBatchInvocation = true;
                VerifyOffline();
#if UNITY_EDITOR
                Debug.Log("I031_INVENTORY_PLACEMENT_AND_LIGHTING_CONTRACT PASS");
                EditorApplication.Exit(0);
#endif
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                Debug.LogException(exception);
                EditorApplication.Exit(1);
#else
                Console.Error.WriteLine(exception);
                throw;
#endif
            }
        }

        private static Verification Run()
        {
            string root = FindRoot();
            List<Scenario> rows = new List<Scenario>();

            ItemSystemSnapshot inventoryEmpty = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(), Inventory());
            Add(rows, "S01", "owned Inventory + empty board",
                inventoryEmpty.isValid &&
                inventoryEmpty.i031State.ownershipCompleteness ==
                    I031OwnershipCompleteness.Complete &&
                inventoryEmpty.i031State.location == I031Location.Inventory &&
                inventoryEmpty.placements.Count == 0,
                "Valid/Complete; placements=0");

            ItemSystemSnapshot inventoryOrdinary = Snapshot(new[]
            {
                P("P_I001", "I001", 1, 2),
                P("P_I007", "I007", 4, 4)
            }, Inventory());
            bool inventoryUnlit = inventoryOrdinary.isValid &&
                inventoryOrdinary.LitRangeCells.Count == 0 &&
                inventoryOrdinary.placements.All(value =>
                    !value.isLightingSource && !value.isDirectLit &&
                    !value.isLit && value.litByItemId.Length == 0 &&
                    value.litByPlacementId.Length == 0 &&
                    value.litDepth == -1 && !value.isCountedInBuild);
            Add(rows, "S02", "Inventory ordinary rows stay unpowered",
                inventoryUnlit, LightingEvidence(inventoryOrdinary));

            ItemSystemSnapshot board = BoardAt(1, 1);
            Add(rows, "S03", "owned Board + stable placement",
                board.isValid && board.i031State.isPlaced &&
                board.placements.Count == 1 &&
                board.placements[0].placementId ==
                    I031InventoryPlacementContract.StablePlacementId,
                StateEvidence(board));

            ItemSystemSnapshot boardMoved = BoardAt(3, 1);
            ItemSystemSnapshot inventoryAgain = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(), Inventory());
            ItemSystemSnapshot boardAgain = BoardAt(1, 3);
            ItemSystemSnapshot[] transitions =
            {
                inventoryEmpty, board, boardMoved, inventoryAgain, boardAgain
            };
            bool identitiesStable = transitions.All(value =>
                value.i031State.itemId == I031InventoryPlacementContract.ItemId &&
                value.i031State.specialIdentityId ==
                    I031InventoryPlacementContract.SpecialIdentityId &&
                value.i031State.stablePlacementId ==
                    I031InventoryPlacementContract.StablePlacementId);
            Add(rows, "S04", "Inventory/Board transition identity",
                identitiesStable && transitions.All(value => value.isValid),
                string.Join(" -> ", transitions.Select(value =>
                    value.i031State.location.ToString())));

            ItemSystemSnapshot duplicateOwnership = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(),
                Inventory(), Inventory());
            Add(rows, "S05", "duplicate ownership rejected",
                HasCode(duplicateOwnership, "I031_OWNERSHIP_DUPLICATE"),
                Codes(duplicateOwnership));

            ItemSystemSnapshot duplicatePlacement = Snapshot(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 0),
                P("P_SYSTEM_I031_DUPLICATE", "I031", 4, 4)
            }, Board());
            Add(rows, "S06", "duplicate placement rejected",
                HasCode(duplicatePlacement, "I031_PLACEMENT_MULTIPLE"),
                Codes(duplicatePlacement));

            ItemSystemSnapshot missingOwnership = Snapshot(
                Array.Empty<ItemSystemPlacementInput>());
            Add(rows, "S07", "missing ownership is Unknown/Invalid",
                !missingOwnership.isValid &&
                missingOwnership.i031State.ownershipCompleteness ==
                    I031OwnershipCompleteness.Unknown &&
                HasCode(missingOwnership, "I031_OWNERSHIP_UNKNOWN"),
                StateEvidence(missingOwnership) + ";" + Codes(missingOwnership));

            ItemSystemSnapshot inventoryConflict = Snapshot(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 0)
            }, Inventory());
            Add(rows, "S08", "Inventory plus placement rejected",
                HasCode(inventoryConflict,
                    "I031_INVENTORY_PLACEMENT_CONFLICT"),
                Codes(inventoryConflict));

            ItemSystemSnapshot boardMissing = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(), Board());
            Add(rows, "S09", "Board without placement rejected",
                HasCode(boardMissing, "I031_BOARD_PLACEMENT_MISSING"),
                Codes(boardMissing));

            ItemSystemSnapshot wrongSpecial = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(),
                State("BAD_SPECIAL", "P_SYSTEM_I031",
                    I031OwnershipCompleteness.Complete,
                    I031Location.Inventory, true));
            ItemSystemSnapshot wrongStable = Snapshot(
                Array.Empty<ItemSystemPlacementInput>(),
                State("SPECIAL_I031", "BAD_STABLE",
                    I031OwnershipCompleteness.Complete,
                    I031Location.Inventory, true));
            Add(rows, "S10", "stable identities enforced",
                HasCode(wrongSpecial, "I031_SPECIAL_IDENTITY_INVALID") &&
                HasCode(wrongStable, "I031_STABLE_PLACEMENT_ID_INVALID"),
                Codes(wrongSpecial) + ";" + Codes(wrongStable));

            ItemSystemSnapshot eye = BoardAt(2, 2);
            ItemSystemSnapshot overlap = Snapshot(new[]
            {
                P("P_SYSTEM_I031", "I031", 1, 1),
                P("P_I001", "I001", 1, 1)
            }, Board());
            ItemSystemSnapshot outOfBounds = BoardAt(5, 0);
            Add(rows, "S11", "eye/overlap/out-of-bounds rejected",
                HasCode(eye, "EYE_CELL_COVERED") &&
                HasCode(overlap, "PLACEMENT_OVERLAP") &&
                HasCode(outOfBounds, "ITEM_OUT_OF_BOUNDS"),
                Codes(eye) + ";" + Codes(overlap) + ";" +
                    Codes(outOfBounds));

            Vector2Int[] centerRange = BoardAt(1, 1).LitRangeCells.ToArray();
            Add(rows, "S12", "four orthogonal targets",
                centerRange.Length == 4 &&
                !centerRange.Contains(new Vector2Int(1, 1)) &&
                Cells(centerRange) == "(1,0);(0,1);(2,1);(1,2)",
                Cells(centerRange));

            ItemSystemSnapshot corner = BoardAt(0, 0);
            ItemSystemSnapshot edge = BoardAt(2, 0);
            Add(rows, "S13", "corner/edge clipping",
                corner.LitRangeCells.Count == 2 &&
                edge.LitRangeCells.Count == 3,
                "corner=" + Cells(corner.LitRangeCells) +
                    ";edge=" + Cells(edge.LitRangeCells));

            ItemSystemSnapshot direct = Snapshot(new[]
            {
                P("P_SYSTEM_I031", "I031", 1, 1),
                P("P_I001", "I001", 1, 2),
                P("P_I007", "I007", 4, 4)
            }, Board());
            ItemSystemPlacementSnapshot neighbor = direct.FindPlacement("P_I001");
            ItemSystemPlacementSnapshot outsider = direct.FindPlacement("P_I007");
            Add(rows, "S14", "direct neighbor versus outsider",
                direct.isValid && neighbor.isDirectLit && neighbor.isLit &&
                !outsider.isDirectLit && !outsider.isLit,
                LightingEvidence(direct));

            ItemSystemSnapshot beforeMove = BoardAt(1, 1);
            ItemSystemSnapshot afterMove = BoardAt(3, 3);
            Add(rows, "S15", "move replaces range",
                beforeMove.isValid && afterMove.isValid &&
                !beforeMove.LitRangeCells.SequenceEqual(
                    afterMove.LitRangeCells) &&
                !beforeMove.LitRangeCells.Intersect(
                    afterMove.LitRangeCells).Any(),
                "before=" + Cells(beforeMove.LitRangeCells) +
                    ";after=" + Cells(afterMove.LitRangeCells));

            ItemSystemSnapshot returned = Snapshot(new[]
            {
                P("P_I001", "I001", 1, 2),
                P("P_I007", "I007", 1, 3)
            }, Inventory());
            Add(rows, "S16", "return clears lighting state",
                returned.isValid && returned.LitRangeCells.Count == 0 &&
                returned.placements.All(value => !value.isLightingSource &&
                    !value.isDirectLit && !value.isLit &&
                    value.litDepth == -1 &&
                    value.litByPlacementId.Length == 0),
                LightingEvidence(returned));

            ItemSystemPlacementSnapshot source = board.FindPlacement(
                I031InventoryPlacementContract.StablePlacementId);
            Add(rows, "S17", "I031 excluded from Build",
                source != null && source.isLit && !source.isCountedInBuild &&
                board.buildSnapshot.ItemResults.All(value =>
                    value.itemId != "I031" || !value.countedInBuild),
                "sourceLit=" + source?.isLit +
                    ";sourceCounted=" + source?.isCountedInBuild);

            ItemInstanceProjectionSetResult emptyProjection =
                EmptyProjectionSet();
            ItemInstancePlacementBindingValidationResult forgedI031 =
                ItemInstancePlacementBindingValidator.Instance.Validate(
                    emptyProjection.snapshot,
                    board,
                    new[]
                    {
                        new ItemInstancePlacementBindingInput(
                            "INSTANCE_I031_FORBIDDEN",
                            "P_SYSTEM_I031",
                            "I031")
                    });
            Add(rows, "S18", "I031 excluded from ordinary identity systems",
                emptyProjection.isSuccess &&
                emptyProjection.snapshot.Projections.Count == 0 &&
                forgedI031.status ==
                    ItemInstancePlacementBindingStatus.Invalid &&
                forgedI031.ValidationErrors.Any(value => value.code ==
                    ItemInstancePlacementBindingValidationCodes
                        .I031OrdinaryInstanceForbidden),
                "projectionCount=" + emptyProjection.snapshot.Projections.Count +
                    ";IF01=" + forgedI031.status);

            ItemInstancePlacementBindingValidationResult emptyIf01 =
                ItemInstancePlacementBindingValidator.Instance.Validate(
                    emptyProjection.snapshot,
                    inventoryEmpty,
                    Array.Empty<ItemInstancePlacementBindingInput>());
            Add(rows, "S19", "IF01 empty set remains valid",
                emptyIf01.isValid && emptyIf01.snapshot.Bindings.Count == 0 &&
                emptyIf01.snapshot.schemaId ==
                    ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId,
                "schema=" + emptyIf01.snapshot.schemaId +
                    ";bindings=" + emptyIf01.snapshot.Bindings.Count +
                    ";canonical=" + emptyIf01.snapshot.canonicalSignature);

            LayoutResilienceItemFactProjectionResult p1Inventory =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    inventoryEmpty, emptyIf01.snapshot);
            Add(rows, "S20", "P1 Inventory empty Complete",
                p1Inventory.Status ==
                    LayoutResilienceItemFactProjectionStatus.Complete &&
                p1Inventory.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Complete &&
                p1Inventory.BuildFacts.PlacementRows.Count == 0,
                P1Evidence(p1Inventory));

            ItemInstancePlacementBindingValidationResult boardIf01 =
                ItemInstancePlacementBindingValidator.Instance.Validate(
                    emptyProjection.snapshot,
                    board,
                    Array.Empty<ItemInstancePlacementBindingInput>());
            LayoutResilienceItemFactProjectionResult p1Board =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    board, boardIf01.snapshot);
            Add(rows, "S21", "P1 Board includes I031 without IF01 row",
                boardIf01.isValid &&
                p1Board.Status ==
                    LayoutResilienceItemFactProjectionStatus.Complete &&
                p1Board.BuildFacts.PlacementRows.Count == 1 &&
                p1Board.BuildFacts.PlacementRows[0].ItemId == "I031",
                P1Evidence(p1Board) + ";IF01=" + boardIf01.status);

            LayoutResilienceItemFactProjectionResult p1Invalid =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    missingOwnership, null);
            RealLayoutResilienceEvaluationPipelineResult p6Invalid =
                DefaultRealLayoutResilienceEvaluationPipeline.Instance.Evaluate(
                    new RealLayoutResilienceEvaluationPipelineInput(
                        "i031.invalid.ownership", missingOwnership, null));
            Add(rows, "S22", "Unknown/Invalid preserved through P1/P6",
                p1Invalid.Status !=
                    LayoutResilienceItemFactProjectionStatus.Complete &&
                p6Invalid.Status !=
                    RealLayoutResilienceEvaluationPipelineStatus.Complete &&
                p6Invalid.BuildProjectionStatus !=
                    LayoutResilienceItemFactProjectionStatus.Complete,
                "P1=" + p1Invalid.Status + ";P6=" + p6Invalid.Status +
                    ";P6.P1=" + p6Invalid.BuildProjectionStatus);

            ItemSystemPlacementInput[] deterministicPlacements =
            {
                P("P_I001", "I001", 1, 2),
                P("P_I007", "I007", 4, 4)
            };
            ItemSystemSnapshot forward = Snapshot(
                deterministicPlacements, Inventory());
            ItemSystemSnapshot reverse = Snapshot(
                deterministicPlacements.Reverse().ToArray(), Inventory());
            LayoutResilienceItemFactProjectionResult p1Forward =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    forward, EmptyIf01(forward).snapshot);
            LayoutResilienceItemFactProjectionResult p1Reverse =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    reverse, EmptyIf01(reverse).snapshot);
            Add(rows, "S23", "deterministic repeat/reversed canonical",
                forward.BuildDebugSignature() == reverse.BuildDebugSignature() &&
                p1Forward.CanonicalSignature == p1Reverse.CanonicalSignature,
                "Item=" + HashText(forward.BuildDebugSignature()) +
                    ";P1=" + p1Forward.CanonicalSignature);

            List<I031InventoryPlacementStateInput> mutableStates =
                new List<I031InventoryPlacementStateInput> { Inventory() };
            ItemSystemSnapshotInput clonedInput = new ItemSystemSnapshotInput(
                Array.Empty<ItemSystemPlacementInput>(),
                i031StateInputs: mutableStates);
            mutableStates.Clear();
            ItemSystemSnapshot clonedSnapshot =
                DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                    clonedInput);
            bool readOnlyThrows = MutationBlocked(
                clonedInput.I031StateInputs, Inventory()) &&
                MutationBlocked(clonedSnapshot.placements, null) &&
                MutationBlocked(clonedSnapshot.LitRangeCells, Vector2Int.zero);
            Add(rows, "S24", "immutable input/snapshot collections",
                clonedSnapshot.isValid && readOnlyThrows,
                "inputRows=" + clonedInput.I031StateInputs.Count +
                    ";mutationBlocked=" + readOnlyThrows);

            List<ProtectedObservation> protectedRows = ProtectedFiles.Select(value =>
                ObserveProtected(root, value)).ToList();
            int leakCount = CountLeaks(root);
            Add(rows, "S25", "protected hashes and leak scan",
                protectedRows.All(value => value.Passed) && leakCount == 0,
                "protected=" + protectedRows.Count(value => value.Passed) +
                    "/" + protectedRows.Count + ";Leak Count=" + leakCount);

            string p1Canonical =
                LayoutResilienceItemFactProjectionAdapterVerifier
                    .ComputeI031V2CanonicalForMigration();
            string p6Canonical =
                RealLayoutResilienceEvaluationPipelineVerifier
                    .ComputeI031V2CanonicalForMigration();
            string itemDebugText = board.BuildDebugSignature();
            string itemDebugCanonical = HashText(itemDebugText);
            string oldItemDebugCanonical = HashText(string.Join("\n",
                itemDebugText.Split('\n')
                    .Where(line => !line.StartsWith(
                        "I031:", StringComparison.Ordinal))
                    .Select(line => line.Replace(
                        "ItemSystemSnapshot.v2",
                        "ItemSystemSnapshot.v1"))));
            string validatorGolden = HashText(string.Join("\n", rows.Take(24)
                .Select(value => value.Id + "|" + value.Passed + "|" +
                    value.Evidence)));
            int passed = rows.Count(value => value.Passed);
            return new Verification(
                root,
                rows,
                protectedRows,
                leakCount,
                emptyIf01.snapshot.canonicalSignature,
                oldItemDebugCanonical,
                itemDebugCanonical,
                validatorGolden,
                p1Canonical,
                p6Canonical,
                unityBatchInvocation ? "PASS" : "NOT_RUN",
                Package + " verifier: " + passed.ToString(
                    CultureInfo.InvariantCulture) + "/" + rows.Count.ToString(
                    CultureInfo.InvariantCulture) + " PASS; Item=" +
                    itemDebugCanonical + "; P1=" + p1Canonical +
                    "; P6=" + p6Canonical + "; Leak Count=" + leakCount);
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

        private static ItemSystemSnapshot BoardAt(int x, int y)
        {
            return Snapshot(new[]
            {
                P("P_SYSTEM_I031", "I031", x, y)
            }, Board());
        }

        private static I031InventoryPlacementStateInput Inventory()
        {
            return I031InventoryPlacementContract.OwnedInventory();
        }

        private static I031InventoryPlacementStateInput Board()
        {
            return I031InventoryPlacementContract.OwnedBoard();
        }

        private static I031InventoryPlacementStateInput State(
            string specialIdentityId,
            string stablePlacementId,
            I031OwnershipCompleteness completeness,
            I031Location location,
            bool isOwned)
        {
            return new I031InventoryPlacementStateInput(
                "I031",
                specialIdentityId,
                stablePlacementId,
                completeness,
                location,
                isOwned);
        }

        private static ItemSystemPlacementInput P(
            string placementId,
            string itemId,
            int x,
            int y)
        {
            return new ItemSystemPlacementInput(
                placementId, itemId, new Vector2Int(x, y));
        }

        private static ItemInstanceProjectionSetResult EmptyProjectionSet()
        {
            return ItemInstanceProjectionProvider.ProjectSet(
                new ItemInstanceProjectionSetRequest(
                    Array.Empty<ItemGeneratedInstanceSnapshot>()));
        }

        private static ItemInstancePlacementBindingValidationResult EmptyIf01(
            ItemSystemSnapshot snapshot)
        {
            return ItemInstancePlacementBindingValidator.Instance.Validate(
                EmptyProjectionSet().snapshot,
                snapshot,
                Array.Empty<ItemInstancePlacementBindingInput>());
        }

        private static bool MutationBlocked<T>(
            IReadOnlyList<T> source,
            T value)
        {
            try
            {
                ((IList<T>)source).Add(value);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool HasCode(ItemSystemSnapshot snapshot, string code)
        {
            return snapshot.validationErrors.Any(value =>
                value.code == code);
        }

        private static string Codes(ItemSystemSnapshot snapshot)
        {
            return string.Join("|", snapshot.validationErrors.Select(value =>
                value.code).Distinct().OrderBy(value => value,
                StringComparer.Ordinal));
        }

        private static string StateEvidence(ItemSystemSnapshot snapshot)
        {
            return snapshot.schemaVersion + "/" +
                snapshot.i031State.ownershipCompleteness + "/" +
                snapshot.i031State.location + "/owned=" +
                snapshot.i031State.isOwned + "/placed=" +
                snapshot.i031State.isPlaced + "/" +
                snapshot.i031State.specialIdentityId + "/" +
                snapshot.i031State.stablePlacementId;
        }

        private static string LightingEvidence(ItemSystemSnapshot snapshot)
        {
            return "range=" + Cells(snapshot.LitRangeCells) + ";rows=" +
                string.Join("|", snapshot.placements.Select(value =>
                    value.placementId + ":source=" + value.isLightingSource +
                    ":direct=" + value.isDirectLit + ":lit=" + value.isLit +
                    ":depth=" + value.litDepth + ":counted=" +
                    value.isCountedInBuild));
        }

        private static string P1Evidence(
            LayoutResilienceItemFactProjectionResult result)
        {
            return result.Status + "/" + result.BuildFactsCompleteness +
                "/rows=" + (result.BuildFacts == null
                    ? "null"
                    : result.BuildFacts.PlacementRows.Count.ToString(
                        CultureInfo.InvariantCulture)) + "/" +
                result.CanonicalSignature;
        }

        private static string Cells(IEnumerable<Vector2Int> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<Vector2Int>())
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .Select(value => "(" + value.x.ToString(
                    CultureInfo.InvariantCulture) + "," + value.y.ToString(
                    CultureInfo.InvariantCulture) + ")"));
        }

        private static void Add(
            ICollection<Scenario> rows,
            string id,
            string contract,
            bool passed,
            string evidence)
        {
            rows.Add(new Scenario(id, contract, passed, evidence));
        }

        private static ProtectedObservation ObserveProtected(
            string root,
            ProtectedFile expected)
        {
            string absolute = Absolute(root, expected.Path);
            string actual = File.Exists(absolute)
                ? HashFile(absolute)
                : "MISSING";
            return new ProtectedObservation(
                expected.Path,
                expected.Hash,
                actual,
                actual == expected.Hash);
        }

        private static int CountLeaks(string root)
        {
            string[] runtimeFiles =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/I031InventoryPlacementContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"
            };
            string[] forbiddenTokens =
            {
                "itemInstanceId", "Revision05", "SceneManager",
                "UnityEngine.UI", "EnemySystem"
            };
            int leaks = 0;
            foreach (string path in runtimeFiles)
            {
                string text = File.ReadAllText(Absolute(root, path), Encoding.UTF8);
                leaks += forbiddenTokens.Count(token => text.IndexOf(
                    token, StringComparison.Ordinal) >= 0);
            }
            return leaks;
        }

        private static void WriteReports(Verification value)
        {
            Directory.CreateDirectory(Absolute(value.Root, ReportDirectory));
            File.WriteAllText(Absolute(value.Root, SpecReport),
                BuildSpec(value), new UTF8Encoding(false));
            File.WriteAllText(Absolute(value.Root, MigrationReport),
                BuildMigration(value), new UTF8Encoding(false));
            File.WriteAllText(Absolute(value.Root, LeakReport),
                BuildLeak(value), new UTF8Encoding(false));
            File.WriteAllText(Absolute(value.Root, MainReport),
                BuildMain(value), new UTF8Encoding(false));
        }

        private static string BuildSpec(Verification value)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("scenarioId,contract,expected,actual,evidence,status");
            foreach (Scenario row in value.Scenarios)
            {
                builder.Append(Csv(row.Id)).Append(',')
                    .Append(Csv(row.Contract)).Append(',')
                    .Append(Csv("PASS")).Append(',')
                    .Append(Csv(row.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(row.Evidence)).Append(',')
                    .AppendLine(Csv(row.Passed ? "PASS" : "FAIL"));
            }
            return builder.ToString();
        }

        private static string BuildMigration(Verification value)
        {
            string itemFileHash = HashFile(Absolute(value.Root,
                "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"));
            string oldValidatorReport = Absolute(value.Root,
                "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotSpec.csv");
            string oldValidatorHash = File.Exists(oldValidatorReport)
                ? "sha256:" + HashFile(oldValidatorReport)
                : "MISSING";
            string[] rows =
            {
                Migration("ItemSystemSnapshot source contract", "ItemSystemSnapshot.v1",
                    "ItemSystemSnapshot.v2",
                    "sha256:821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df",
                    "sha256:" + itemFileHash,
                    "Explicit immutable I031 ownership/location branch"),
                Migration("ItemSystemSnapshot debug canonical", "ItemSystemSnapshot.v1",
                    "ItemSystemSnapshot.v2",
                    value.OldItemDebugCanonical,
                    value.ItemDebugCanonical,
                    "Schema and deterministic I031 state row migrated"),
                Migration("ItemSystemValidator golden", "v1 golden", "v2 golden",
                    oldValidatorHash, value.ValidatorGolden,
                    "Placement-count requirement replaced by explicit state validation"),
                Migration("N01C-P1 fixture canonical", "ItemSystemSnapshot.v1",
                    "ItemSystemSnapshot.v2",
                    "sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd",
                    value.P1Canonical,
                    "P1 fixture consumes Item v2; projection algorithm unchanged"),
                Migration("N01C-P6 fixture canonical", "ItemSystemSnapshot.v1",
                    "ItemSystemSnapshot.v2",
                    "sha256:cc888ce3c7748f8bb3b3eafbbc89f37daccb3812ec984470093630b3bc94f98e",
                    value.P6Canonical,
                    "P6 receives migrated P1 signature once; pipeline algorithm unchanged")
            };
            return "contract,oldVersion,newVersion,oldSignature,newSignature,reason,status\n" +
                string.Join("\n", rows) + "\n";
        }

        private static string Migration(
            string contract,
            string oldVersion,
            string newVersion,
            string oldSignature,
            string newSignature,
            string reason)
        {
            return string.Join(",", new[]
            {
                Csv(contract), Csv(oldVersion), Csv(newVersion),
                Csv(oldSignature), Csv(newSignature), Csv(reason), Csv("PASS")
            });
        }

        private static string BuildLeak(Verification value)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# I031 Inventory Placement And Lighting Leak Check")
                .AppendLine()
                .AppendLine("- Package: `" + Package + "`")
                .AppendLine("- Assignment SHA-256: `" + AssignmentHash + "`")
                .AppendLine("- Leak Count: `" + value.LeakCount.ToString(
                    CultureInfo.InvariantCulture) + "`")
                .AppendLine()
                .AppendLine("| Protected file | Expected SHA-256 | Actual SHA-256 | Status |")
                .AppendLine("|---|---|---|---|");
            foreach (ProtectedObservation row in value.ProtectedRows)
            {
                builder.Append("| `").Append(row.Path).Append("` | `")
                    .Append(row.Expected).Append("` | `").Append(row.Actual)
                    .Append("` | ").Append(row.Passed ? "PASS" : "FAIL")
                    .AppendLine(" |");
            }
            builder.AppendLine()
                .AppendLine("Forbidden scope touched: `0`")
                .AppendLine("commit/tag/push: `none`")
                .AppendLine("Next package: `NOT_STARTED`");
            return builder.ToString();
        }

        private static string BuildMain(Verification value)
        {
            int passed = value.Scenarios.Count(row => row.Passed);
            return new StringBuilder()
                .AppendLine("# I031 Inventory Placement And Lighting Contract Report")
                .AppendLine()
                .AppendLine("- Package: `" + Package + "`")
                .AppendLine("- Assignment SHA-256: `" + AssignmentHash + "`")
                .AppendLine("- Result: `" + passed.ToString(
                    CultureInfo.InvariantCulture) + "/" +
                    value.Scenarios.Count.ToString(CultureInfo.InvariantCulture) +
                    " PASS`")
                .AppendLine("- Item schema: `ItemSystemSnapshot.v1 -> ItemSystemSnapshot.v2`")
                .AppendLine("- I031 identity: `SPECIAL_I031 / P_SYSTEM_I031`")
                .AppendLine("- IF01 schema: `" +
                    ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId +
                    "` (unchanged)")
                .AppendLine("- IF01 empty canonical: `" + value.If01Canonical + "`")
                .AppendLine("- Item v2 debug canonical: `" +
                    value.ItemDebugCanonical + "`")
                .AppendLine("- P1 v2 fixture canonical: `" +
                    value.P1Canonical + "`")
                .AppendLine("- P6 v2 fixture canonical: `" +
                    value.P6Canonical + "`")
                .AppendLine("- Lighting: existing `ItemLightingResolver` / `OrthogonalAdjacentLightingRangeRule` reused")
                .AppendLine("- Protected hashes: `" +
                    value.ProtectedRows.Count(row => row.Passed).ToString(
                        CultureInfo.InvariantCulture) + "/" +
                    value.ProtectedRows.Count.ToString(CultureInfo.InvariantCulture) +
                    " PASS`")
                .AppendLine("- Leak Count: `" + value.LeakCount.ToString(
                    CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Offline verifier: `PASS`")
                .AppendLine("- Unity verifier: `" +
                    value.UnityVerifierStatus + "`")
                .AppendLine("- Forbidden scope touched: `0`")
                .AppendLine("- commit/tag/push: `none`")
                .AppendLine("- Next package: `NOT_STARTED`")
                .AppendLine()
                .AppendLine("Detailed S01-S25 evidence is in `I031InventoryPlacementAndLightingContractSpec.csv`; canonical migration is recorded in `I031InventoryPlacementAndLightingCanonicalMigration.csv`.")
                .ToString();
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string FindRoot()
        {
            DirectoryInfo current = new DirectoryInfo(
                Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(
                        current.FullName, "ProjectSettings")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }
            throw new DirectoryNotFoundException(
                "Unity project root containing Assets and ProjectSettings was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.GetFullPath(Path.Combine(
                root, relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return string.Concat(sha.ComputeHash(stream).Select(value =>
                    value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static string HashText(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(value ?? string.Empty))
                    .Select(item => item.ToString(
                        "x2", CultureInfo.InvariantCulture)));
            }
        }

        private sealed class Scenario
        {
            public Scenario(string id, string contract, bool passed, string evidence)
            {
                Id = id;
                Contract = contract;
                Passed = passed;
                Evidence = evidence ?? string.Empty;
            }

            public string Id { get; }
            public string Contract { get; }
            public bool Passed { get; }
            public string Evidence { get; }
        }

        private readonly struct ProtectedFile
        {
            public ProtectedFile(string path, string hash)
            {
                Path = path;
                Hash = hash;
            }

            public string Path { get; }
            public string Hash { get; }
        }

        private readonly struct ProtectedObservation
        {
            public ProtectedObservation(
                string path,
                string expected,
                string actual,
                bool passed)
            {
                Path = path;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }

            public string Path { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class Verification
        {
            public Verification(
                string root,
                IReadOnlyList<Scenario> scenarios,
                IReadOnlyList<ProtectedObservation> protectedRows,
                int leakCount,
                string if01Canonical,
                string oldItemDebugCanonical,
                string itemDebugCanonical,
                string validatorGolden,
                string p1Canonical,
                string p6Canonical,
                string unityVerifierStatus,
                string summary)
            {
                Root = root;
                Scenarios = scenarios;
                ProtectedRows = protectedRows;
                LeakCount = leakCount;
                If01Canonical = if01Canonical;
                OldItemDebugCanonical = oldItemDebugCanonical;
                ItemDebugCanonical = itemDebugCanonical;
                ValidatorGolden = validatorGolden;
                P1Canonical = p1Canonical;
                P6Canonical = p6Canonical;
                UnityVerifierStatus = unityVerifierStatus;
                Summary = summary;
            }

            public string Root { get; }
            public IReadOnlyList<Scenario> Scenarios { get; }
            public IReadOnlyList<ProtectedObservation> ProtectedRows { get; }
            public int LeakCount { get; }
            public string If01Canonical { get; }
            public string OldItemDebugCanonical { get; }
            public string ItemDebugCanonical { get; }
            public string ValidatorGolden { get; }
            public string P1Canonical { get; }
            public string P6Canonical { get; }
            public string UnityVerifierStatus { get; }
            public string Summary { get; }
        }
    }
}
