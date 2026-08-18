using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalItemDetailProjectionAndSelectionTests
    {
        public const string TerminalMarker =
            "C1_FORMAL_ITEM_DETAIL_PROJECTION_AND_SELECTION_FOCUSED_TESTS_PASS";

        private const string PresenterSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemArrangementPresenter.cs";
        private const string BoardSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemBoardView.cs";
        private const string SeamSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1FormalItemDetailProjectionAndSelection.cs";

        private static readonly Sprite ArtworkToken = CreateArtworkToken();
        private static int assertionCount;

        [MenuItem(
            "Tools/TalismanBag/QA/Canonical Item Detail Player Semantics Focused Verify")]
        public static void RunFromMenu()
        {
            RunAllOrThrow();
            Debug.Log(TerminalMarker + " / assertions="
                + assertionCount.ToString(CultureInfo.InvariantCulture));
        }

        public static int Main()
        {
            return RunFocused();
        }

        public static int RunFocused()
        {
            try
            {
                RunAllOrThrow();
                Console.WriteLine(TerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_FORMAL_ITEM_DETAIL_PROJECTION_AND_SELECTION_FOCUSED_TESTS_FAIL "
                    + exception);
                return 1;
            }
        }

        public static void RunAllOrThrow()
        {
            assertionCount = 0;
            VerifyPublicSurfaceAndStaticBoundary();
            VerifyCanonicalOnlyOrdinaryItemDetails();
            VerifyI031SystemDetail();
            VerifySameBaseInstancesRemainDistinct();
        }

        private static void VerifyPublicSurfaceAndStaticBoundary()
        {
            Type presenter = typeof(
                C1ExactBattleSandboxItemArrangementPresenter);
            PropertyInfo current = presenter.GetProperty(
                "CurrentFormalItemDetail",
                BindingFlags.Instance | BindingFlags.Public);
            Require(current != null
                    && current.PropertyType
                    == typeof(C1FormalItemDetailSelectionResult),
                "PRESENTER_CURRENT_DETAIL_PUBLIC_SURFACE_MISSING");
            EventInfo changed = presenter.GetEvent(
                "FormalItemDetailChanged",
                BindingFlags.Instance | BindingFlags.Public);
            Require(changed != null
                    && changed.EventHandlerType
                    == typeof(Action<C1FormalItemDetailSelectionResult>),
                "PRESENTER_DETAIL_EVENT_PUBLIC_SURFACE_MISSING");
            RequireExactPublicMethod(
                presenter,
                "OpenSelectedFormalItemDetail",
                typeof(C1FormalItemDetailSelectionResult),
                typeof(C1FormalItemDetailSelectionRequest));
            RequireExactPublicMethod(
                presenter,
                "CloseFormalItemDetail",
                typeof(C1FormalItemDetailSelectionResult),
                typeof(C1FormalItemDetailSelectionRequest));
            RequireExactPublicMethod(
                presenter,
                "Bind",
                typeof(bool),
                typeof(C1FormalItemSessionAuthority),
                typeof(string).MakeByRefType());
            Require(typeof(IC1FormalItemArtworkResolver).IsAssignableFrom(
                    typeof(C1ExactBattleSandboxItemBoardView)),
                "BOARD_ARTWORK_RESOLVER_INTERFACE_MISSING");
            PropertyInfo combatFacts = typeof(C1FormalItemDetailProjection)
                .GetProperty(
                    "combatFacts",
                    BindingFlags.Instance | BindingFlags.Public);
            Require(combatFacts != null
                    && combatFacts.PropertyType
                    == typeof(C1FormalItemCombatDetailFacts),
                "FORMAL_COMBAT_DETAIL_FACTS_PUBLIC_SURFACE_MISSING");
            foreach (string propertyName in new[]
                     {
                         "schemaId",
                         "productContext",
                         "baseItemId",
                         "rarityKey",
                         "rarityVersionKey",
                         "profileRevision",
                         "directDamage",
                         "cooldownSeconds",
                         "cooldownUnit",
                         "lightingRequirement",
                         "isPlaced",
                         "isLit",
                         "contributionState",
                         "sourceProjectionCanonicalSignature",
                         "combatSourceKind",
                         "familyId",
                         "familyVariantId",
                         "effectCategoryId",
                         "triggerId",
                         "activationConditionId",
                         "targetScopeId",
                         "cueIdentity",
                         "cadenceKind",
                         "cadenceMilliseconds",
                         "firstOffsetMilliseconds",
                         "minimumLayoutTier",
                         "factMaturity",
                         "sourceSnapshotCanonicalSignature",
                         "sourceProfileCanonicalSignature",
                         "canonicalSignature"
                     })
                Require(typeof(C1FormalItemCombatDetailFacts).GetProperty(
                            propertyName,
                            BindingFlags.Instance | BindingFlags.Public) != null,
                    "FORMAL_COMBAT_DETAIL_PUBLIC_FIELD_MISSING "
                    + propertyName);

            string presenterSource = ReadRequiredSource(PresenterSourcePath);
            string combined = presenterSource
                              + ReadRequiredSource(BoardSourcePath)
                              + ReadRequiredSource(SeamSourcePath);
            foreach (string forbidden in new[]
                     {
                         "BuildSandbox",
                         "Resources.Load",
                         "GameObject.Find",
                         "new GameObject",
                         "SubmitExactlyOne(\n                C1FormalItemDetail",
                         "displayItemName ==",
                         "displayItemName,"
                     })
                Require(combined.IndexOf(
                            forbidden,
                            StringComparison.Ordinal) < 0,
                    "FORBIDDEN_RUNTIME_BOUNDARY_TOKEN " + forbidden);
            int nullGate = presenterSource.IndexOf(
                "if (result == null) return;",
                StringComparison.Ordinal);
            int acceptedGate = presenterSource.IndexOf(
                "if (!result.accepted)",
                StringComparison.Ordinal);
            int changedGate = presenterSource.IndexOf(
                "if (!result.changed) return;",
                StringComparison.Ordinal);
            int stateAssignment = presenterSource.IndexOf(
                "currentFormalItemDetail = result;",
                StringComparison.Ordinal);
            int eventNotification = presenterSource.IndexOf(
                "NotifyFormalItemDetailChanged(result);",
                StringComparison.Ordinal);
            Require(nullGate >= 0
                    && acceptedGate > nullGate
                    && changedGate > acceptedGate
                    && stateAssignment > changedGate
                    && eventNotification > stateAssignment,
                "PRESENTER_EVENT_CHANGED_ONLY_GATE_MISSING");
            Require(combined.Contains(
                    "if (clearSubscribers) formalItemDetailChanged = null;"),
                "PRESENTER_SUBSCRIBER_CLEANUP_MISSING");
            Require(combined.IndexOf(
                        "IItemDetailViewModelProvider",
                        StringComparison.Ordinal) < 0,
                "FORMAL_DETAIL_LEGACY_PROVIDER_REACHABLE");
            Require(combined.IndexOf(
                        "C1Lv1StarterItemBaselineCatalog.TryGetProjection",
                        StringComparison.Ordinal) < 0,
                "FORMAL_DETAIL_STARTER_FALLBACK_REACHABLE");
            Require(combined.IndexOf(
                        "C1CampaignPool15BaseCombatFacts.Resolve()",
                        StringComparison.Ordinal) < 0,
                "FORMAL_DETAIL_POOL15_FALLBACK_REACHABLE");
        }

        private static void VerifyCanonicalOnlyOrdinaryItemDetails()
        {
            MethodInfo validateVisibleProjection = typeof(
                    C1ExactBattleSandboxItemDetailPresenter)
                .GetMethod(
                    "ValidateFormalProjection",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Require(validateVisibleProjection != null,
                "VISIBLE_DETAIL_PROJECTION_GATE_MISSING");

            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    ItemBalanceWorkbenchCatalog.CatalogAssetPath);
            Require(source != null, "CANONICAL_DETAIL_CATALOG_ASSET_MISSING");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    source,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "CANONICAL_DETAIL_RESOLVER_REJECTED "
                + string.Join(";", errors ?? Array.Empty<string>()));

            bool observedAcceptedNonDamageProjection = false;
            string[] baseItemIds = { "I001", "I007", "I016", "I020" };
            for (int index = 0; index < baseItemIds.Length; index++)
            {
                string baseItemId = baseItemIds[index];
                string itemInstanceId = "focused.canonical.detail."
                                        + baseItemId;
                CanonicalItemDefinition definition = resolver.GetDefinition(
                    baseItemId);
                Require(definition != null && definition.isOrdinaryDropEligible,
                    "CANONICAL_DETAIL_DEFINITION_MISSING " + baseItemId);
                ItemInstanceRollResult roll = CanonicalItemInstanceFactory.Create(
                    resolver,
                    itemInstanceId,
                    baseItemId,
                    ItemInstanceRarity.White,
                    810000L + index);
                Require(roll?.isSuccess == true && roll.snapshot != null,
                    "CANONICAL_DETAIL_INSTANCE_ROLL_FAILED " + baseItemId);
                C1FormalItemSessionSnapshot snapshot =
                    CreateCanonicalFixtureSnapshot(
                        "focused-canonical-detail-" + baseItemId,
                        roll.snapshot,
                        definition);

                C1FormalItemDetailSelectionResult result =
                    C1FormalItemDetailProjectionAndSelection.Evaluate(
                        C1FormalItemDetailSelectionRequest.Open(
                            snapshot,
                            itemInstanceId),
                        snapshot,
                        new FakeArtworkResolver(),
                        C1FormalItemDetailSelectionResult.InitialClosed());
                Require(result?.accepted == true
                        && result.isOpen
                        && result.projection != null,
                    "CANONICAL_DETAIL_OPEN_REJECTED " + baseItemId
                    + " / " + result?.diagnostic);
                ItemDetailViewModel model = result.projection
                    .CreateViewModelClone();
                bool visibleProjectionAccepted = (bool)
                    validateVisibleProjection.Invoke(
                        null,
                        new object[] { result.projection, model });
                Require(visibleProjectionAccepted,
                    "VISIBLE_DETAIL_PROJECTION_REJECTED " + baseItemId);
                if (!result.projection.combatFacts.directDamage.HasValue)
                    observedAcceptedNonDamageProjection = true;
                Equal(definition.displayName,
                    model.displayItemName,
                    "canonical display name " + baseItemId);
                Equal(itemInstanceId,
                    model.itemInstanceId,
                    "canonical instance identity " + baseItemId);
                Equal(definition.combatEffect.triggerEventId,
                    result.projection.combatFacts.triggerId,
                    "canonical trigger identity " + baseItemId);
                Equal(C1FormalItemCombatDetailFacts.CanonicalSourceKind,
                    result.projection.combatFacts.combatSourceKind,
                    "canonical combat source " + baseItemId);
                Require(!string.IsNullOrWhiteSpace(model.displayTriggerText)
                        && model.displayTriggerText.IndexOf(
                            "on_",
                            StringComparison.Ordinal) < 0,
                    "CANONICAL_DETAIL_PLAYER_TRIGGER_UNREADABLE "
                    + baseItemId);
                Require(model.displayBasicEffects.Any(line =>
                        line != null
                        && !string.IsNullOrWhiteSpace(line.body)
                        && line.body.IndexOf(
                            "作用对象：",
                            StringComparison.Ordinal) >= 0),
                    "CANONICAL_DETAIL_PLAYER_EFFECT_UNREADABLE "
                    + baseItemId);
                foreach (ItemGeneratedStatSnapshot stat in
                         roll.snapshot.GeneratedStats)
                {
                    Require(model.displayPrimaryStats.Any(line =>
                            line != null
                            && string.Equals(
                                line.label,
                                ExpectedPlayerStatLabel(stat.statId),
                                StringComparison.Ordinal)
                            && string.Equals(
                                line.value,
                                ExpectedPlayerStatValue(
                                    stat.statId,
                                    stat.rawUnits),
                                StringComparison.Ordinal)),
                        "CANONICAL_DETAIL_STAT_MISSING " + baseItemId
                        + "/" + stat.statId);
                }
            }
            Require(observedAcceptedNonDamageProjection,
                "VISIBLE_DETAIL_NON_DAMAGE_PROJECTION_NOT_PROVEN");
        }

        private static void VerifySameBaseInstancesRemainDistinct()
        {
            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    ItemBalanceWorkbenchCatalog.CatalogAssetPath);
            Require(source != null,
                "DUPLICATE_DETAIL_CATALOG_ASSET_MISSING");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    source,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "DUPLICATE_DETAIL_RESOLVER_REJECTED "
                + string.Join(";", errors ?? Array.Empty<string>()));
            CanonicalItemDefinition definition = resolver.GetDefinition("I020");
            Require(definition != null && definition.isOrdinaryDropEligible,
                "DUPLICATE_DETAIL_DEFINITION_MISSING");
            ItemInstanceRollResult firstRoll = CanonicalItemInstanceFactory.Create(
                resolver,
                "focused.duplicate.detail.first",
                definition.baseItemId,
                ItemInstanceRarity.White,
                820001L);
            ItemInstanceRollResult secondRoll = CanonicalItemInstanceFactory.Create(
                resolver,
                "focused.duplicate.detail.second",
                definition.baseItemId,
                ItemInstanceRarity.White,
                820002L);
            Require(firstRoll?.isSuccess == true
                    && secondRoll?.isSuccess == true,
                "DUPLICATE_DETAIL_INSTANCE_ROLL_FAILED");

            C1FormalItemRosterEntrySnapshot firstRoster = CreateRosterEntry(
                firstRoll.snapshot,
                definition);
            C1FormalItemRosterEntrySnapshot secondRoster = CreateRosterEntry(
                secondRoll.snapshot,
                definition);
            Vector2Int boardCell = Vector2Int.zero;
            C1FormalItemPlacementSnapshot formalPlacement =
                InvokeInternalConstructor<C1FormalItemPlacementSnapshot>(
                    new[]
                    {
                        typeof(string), typeof(string), typeof(string),
                        typeof(Vector2Int), typeof(int)
                    },
                    firstRoster.itemInstanceId,
                    firstRoster.itemInstanceId,
                    firstRoster.baseItemId,
                    boardCell,
                    0);
            ItemSystemPlacementSnapshot itemSystemPlacement = new(
                firstRoster.itemInstanceId,
                firstRoster.baseItemId,
                definition.displayName,
                boardCell,
                0,
                new[] { boardCell },
                boardCell,
                false,
                false,
                false,
                string.Empty,
                string.Empty,
                0,
                Array.Empty<Vector2Int>(),
                false,
                false,
                false,
                1,
                1,
                Array.Empty<string>(),
                Array.Empty<string>());
            ItemSystemSnapshot itemSystem = new(
                5,
                new Vector2Int(2, 2),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                new[] { itemSystemPlacement },
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                null,
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                null,
                string.Empty,
                false,
                string.Empty,
                Array.Empty<ItemSystemValidationError>());
            C1FormalItemTrayLayoutSnapshot trayLayout =
                InvokeInternalConstructor<C1FormalItemTrayLayoutSnapshot>(
                    new[]
                    {
                        typeof(IEnumerable<C1FormalItemTrayPlacementSnapshot>)
                    },
                    (object)Array.Empty<C1FormalItemTrayPlacementSnapshot>());
            C1FormalItemSessionSnapshot snapshot =
                InvokeInternalConstructor<C1FormalItemSessionSnapshot>(
                    new[]
                    {
                        typeof(string), typeof(long),
                        typeof(IEnumerable<C1FormalItemRosterEntrySnapshot>),
                        typeof(IEnumerable<C1FormalItemPlacementSnapshot>),
                        typeof(C1FormalItemTrayLayoutSnapshot),
                        typeof(ItemSystemSnapshot), typeof(string)
                    },
                    "focused-duplicate-detail",
                    1L,
                    new[] { firstRoster, secondRoster },
                    new[] { formalPlacement },
                    trayLayout,
                    itemSystem,
                    "focused.duplicate.detail");

            C1FormalItemDetailSelectionResult secondDetail =
                C1FormalItemDetailProjectionAndSelection.Evaluate(
                    C1FormalItemDetailSelectionRequest.Open(
                        snapshot,
                        secondRoster.itemInstanceId),
                    snapshot,
                    new FakeArtworkResolver(),
                    C1FormalItemDetailSelectionResult.InitialClosed());
            Require(secondDetail?.accepted == true
                    && secondDetail.isOpen
                    && secondDetail.projection != null,
                "SAME_BASE_TRAY_INSTANCE_DETAIL_REJECTED "
                + secondDetail?.diagnostic);
            Equal(secondRoster.itemInstanceId,
                secondDetail.projection.itemInstanceId,
                "same-base Tray detail keeps exact second instance");
        }

        private static void VerifyI031SystemDetail()
        {
            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    ItemBalanceWorkbenchCatalog.CatalogAssetPath);
            Require(source != null, "I031_DETAIL_CATALOG_ASSET_MISSING");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    source,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "I031_DETAIL_RESOLVER_REJECTED "
                + string.Join(";", errors ?? Array.Empty<string>()));
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    "focused-i031-detail",
                    1L,
                    resolver);
            Require(creation?.isSuccess == true
                    && creation.authority?.Current != null,
                "I031_DETAIL_SESSION_CREATE_REJECTED "
                + creation?.diagnosticCode);

            C1FormalItemSessionSnapshot snapshot = creation.authority.Current;
            C1FormalItemDetailSelectionResult result =
                C1FormalItemDetailProjectionAndSelection.Evaluate(
                    C1FormalItemDetailSelectionRequest.Open(
                        snapshot,
                        I031InventoryPlacementContract.SpecialIdentityId),
                    snapshot,
                    new FakeArtworkResolver(),
                    C1FormalItemDetailSelectionResult.InitialClosed());
            Require(result?.accepted == true
                    && result.isOpen
                    && result.projection != null,
                "I031_DETAIL_OPEN_REJECTED " + result?.diagnostic);
            ItemDetailViewModel model = result.projection
                .CreateViewModelClone();
            Require(string.Equals(
                    model.displayItemName,
                    "聚念石",
                    StringComparison.Ordinal)
                    && string.Equals(
                        model.itemInstanceId,
                        I031InventoryPlacementContract.SpecialIdentityId,
                        StringComparison.Ordinal),
                "I031_DETAIL_IDENTITY_OR_NAME_INVALID");
            Require(result.projection.combatFacts != null
                    && !result.projection.combatFacts.isApplicable,
                "I031_DETAIL_MUST_NOT_BECOME_ORDINARY_COMBAT_ITEM");
            Require(model.displayPrimaryStats.Any(line =>
                        line != null
                        && string.Equals(
                            line.label,
                            "念力上限",
                            StringComparison.Ordinal)
                        && string.Equals(
                            line.value,
                            C1FormalI031NianCapacityProjection.MaxNian
                                .ToString(CultureInfo.InvariantCulture),
                            StringComparison.Ordinal))
                    && model.displayPrimaryStats.Any(line =>
                        line != null
                        && string.Equals(
                            line.label,
                            "念力产出",
                            StringComparison.Ordinal)),
                "I031_DETAIL_NIAN_FACTS_MISSING");
            MethodInfo validateVisibleProjection = typeof(
                    C1ExactBattleSandboxItemDetailPresenter)
                .GetMethod(
                    "ValidateFormalProjection",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Require(validateVisibleProjection != null
                    && (bool)validateVisibleProjection.Invoke(
                        null,
                        new object[] { result.projection, model }),
                "VISIBLE_I031_DETAIL_PROJECTION_REJECTED");
        }

        private static string ExpectedPlayerStatLabel(string statId)
        {
            return statId switch
            {
                "damage" => "伤害",
                "break" => "破壳",
                "control" => "控制强度",
                "cooldown" => "触发间隔",
                "nianCost" => "念力消耗",
                "duration" => "持续时间",
                "guard" => "护势",
                "cleanse" => "净化层数",
                "heal" => "治疗",
                _ => "效果数值"
            };
        }

        private static string ExpectedPlayerStatValue(
            string statId,
            long rawUnits)
        {
            string value = rawUnits.ToString(CultureInfo.InvariantCulture);
            return statId == "cooldown" || statId == "duration"
                ? value + " 秒"
                : value;
        }

        private static C1FormalItemSessionSnapshot
            CreateCanonicalFixtureSnapshot(
                string sessionToken,
                ItemGeneratedInstanceSnapshot generated,
                CanonicalItemDefinition definition)
        {
            C1FormalItemRosterEntrySnapshot roster = CreateRosterEntry(
                generated,
                definition);
            ItemSystemSnapshot itemSystem = new ItemSystemSnapshot(
                5,
                new Vector2Int(2, 2),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                Array.Empty<ItemSystemPlacementSnapshot>(),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                null,
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                null,
                string.Empty,
                false,
                string.Empty,
                Array.Empty<ItemSystemValidationError>());
            C1FormalItemTrayLayoutSnapshot trayLayout =
                InvokeInternalConstructor<C1FormalItemTrayLayoutSnapshot>(
                    new[]
                    {
                        typeof(IEnumerable<C1FormalItemTrayPlacementSnapshot>)
                    },
                    (object)Array.Empty<C1FormalItemTrayPlacementSnapshot>());
            return InvokeInternalConstructor<C1FormalItemSessionSnapshot>(
                new[]
                {
                    typeof(string),
                    typeof(long),
                    typeof(IEnumerable<C1FormalItemRosterEntrySnapshot>),
                    typeof(IEnumerable<C1FormalItemPlacementSnapshot>),
                    typeof(C1FormalItemTrayLayoutSnapshot),
                    typeof(ItemSystemSnapshot),
                    typeof(string)
                },
                sessionToken,
                1L,
                new[] { roster },
                Array.Empty<C1FormalItemPlacementSnapshot>(),
                trayLayout,
                itemSystem,
                "focused.canonical.detail");
        }

        private static C1FormalItemRosterEntrySnapshot CreateRosterEntry(
            ItemGeneratedInstanceSnapshot generated,
            CanonicalItemDefinition definition)
        {
            ItemInstanceIdentitySnapshot identity = generated.identity;
            C1FormalItemOrdinaryInstanceSnapshot ordinary =
                InvokeInternalConstructor<C1FormalItemOrdinaryInstanceSnapshot>(
                    new[]
                    {
                        typeof(ItemInstanceIdentitySnapshot),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(ItemGeneratedInstanceSnapshot),
                        typeof(CanonicalItemDefinition)
                    },
                    identity,
                    identity.BuildCanonicalSignature(),
                    string.Empty,
                    CanonicalItemCatalogContract.CatalogId,
                    "FOCUSED_CANONICAL_DETAIL",
                    generated.BuildCanonicalSignature(),
                    generated,
                    definition);
            return InvokeInternalConstructor<C1FormalItemRosterEntrySnapshot>(
                    new[]
                    {
                        typeof(string),
                        typeof(string),
                        typeof(bool),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(C1FormalItemOrdinaryInstanceSnapshot)
                    },
                    generated.itemInstanceId,
                    generated.baseItemId,
                    false,
                    generated.rarity.ToStableKey(),
                    ordinary.baselineProjectionCanonicalSignature,
                    ordinary.itemCatalogCanonicalSignature,
                    ordinary.instanceCanonicalSignature,
                    ordinary);
        }

        private static T InvokeInternalConstructor<T>(
            Type[] signature,
            params object[] arguments)
        {
            const BindingFlags flags = BindingFlags.Instance
                                       | BindingFlags.NonPublic;
            ConstructorInfo constructor = typeof(T).GetConstructor(
                flags,
                null,
                signature,
                null);
            string diagnostic = "REFLECTION_CONSTRUCTOR_"
                                + typeof(T).Name.ToUpperInvariant();
            Require(constructor != null,
                diagnostic + "_SIGNATURE_MISSING");
            try
            {
                return (T)constructor.Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException(
                    diagnostic + "_INVOKE_FAILED",
                    exception.InnerException ?? exception);
            }
        }

        private static void RequireExactPublicMethod(
            Type declaringType,
            string name,
            Type returnType,
            params Type[] parameters)
        {
            MethodInfo method = declaringType.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public,
                null,
                parameters,
                null);
            Require(method != null && method.ReturnType == returnType,
                "PUBLIC_METHOD_SIGNATURE_MISSING "
                + declaringType.FullName + "." + name);
        }

        private static string ReadRequiredSource(string path)
        {
            Require(File.Exists(path), "REQUIRED_SOURCE_MISSING " + path);
            return File.ReadAllText(path);
        }

        private static Sprite CreateArtworkToken()
        {
            return Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }

        private static void Require(bool condition, string message)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            Require(EqualityComparer<T>.Default.Equals(expected, actual),
                label + " expected=" + expected + " actual=" + actual);
        }

        private sealed class FakeArtworkResolver :
            IC1FormalItemArtworkResolver
        {
            private readonly bool resolves;
            private readonly bool wrongIdentity;

            public FakeArtworkResolver(
                bool resolves = true,
                bool wrongIdentity = false)
            {
                this.resolves = resolves;
                this.wrongIdentity = wrongIdentity;
            }

            public bool TryResolveFormalItemArtwork(
                string authoritativeBaseItemId,
                C1FormalItemArtworkLightingState lightingState,
                out Sprite artwork,
                out string artworkIdentity)
            {
                artwork = resolves ? ArtworkToken : null;
                artworkIdentity = resolves
                    ? wrongIdentity
                        ? "wrong.identity"
                        : C1FormalItemArtworkIdentity.Create(
                            authoritativeBaseItemId,
                            lightingState)
                    : string.Empty;
                return resolves;
            }
        }
    }
}
