using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.Normalization
{
    public static class EnemyValidationNormalizationSchema
    {
        public const string SchemaId = "EnemyValidationContentSnapshot.v1";
        public const int SchemaVersion = 1;
    }

    public enum ValidationCarrierKind { Enemy = 0, Boss = 1 }
    public enum ValidationProfileKind { Enemy = 0, Boss = 1 }
    public enum NormalizationExceptionKind { Carrier = 0, MechanicProfile = 1 }

    public sealed class LegacyVocabularyToken
    {
        public LegacyVocabularyToken(string sourceKind, string legacyKey)
        { SourceKind = R.Text(sourceKind); LegacyKey = R.Text(legacyKey); }
        public string SourceKind { get; }
        public string LegacyKey { get; }
    }

    public sealed class CarrierNormalizationSource
    {
        public CarrierNormalizationSource(ValidationCarrierKind kind, string id, string displayName, string legacyType,
            IEnumerable<LegacyVocabularyToken> tokens, IEnumerable<string> legacyFacts,
            bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
        {
            Kind = kind; Id = R.Text(id); DisplayName = R.Text(displayName); LegacyType = R.Text(legacyType);
            Tokens = R.Freeze(tokens); LegacyFacts = R.Strings(legacyFacts);
            DevOnly = devOnly; IsEnabled = isEnabled; EntersFormalFlow = entersFormalFlow;
        }
        public ValidationCarrierKind Kind { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public string LegacyType { get; }
        public IReadOnlyList<LegacyVocabularyToken> Tokens { get; }
        public IReadOnlyList<string> LegacyFacts { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class MechanicProfileNormalizationSource
    {
        public MechanicProfileNormalizationSource(ValidationProfileKind kind, string id, string displayName,
            IEnumerable<LegacyVocabularyToken> publicTokens, IEnumerable<LegacyVocabularyToken> requiredTokens,
            IEnumerable<LegacyVocabularyToken> optionalTokens, IEnumerable<LegacyVocabularyToken> counterTokens,
            IEnumerable<string> legacyFacts, IEnumerable<string> sourceReferences,
            bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
        {
            Kind = kind; Id = R.Text(id); DisplayName = R.Text(displayName);
            PublicTokens = R.Freeze(publicTokens); RequiredTokens = R.Freeze(requiredTokens);
            OptionalTokens = R.Freeze(optionalTokens); CounterTokens = R.Freeze(counterTokens);
            LegacyFacts = R.Strings(legacyFacts); SourceReferences = R.Strings(sourceReferences);
            DevOnly = devOnly; IsEnabled = isEnabled; EntersFormalFlow = entersFormalFlow;
        }
        public ValidationProfileKind Kind { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<LegacyVocabularyToken> PublicTokens { get; }
        public IReadOnlyList<LegacyVocabularyToken> RequiredTokens { get; }
        public IReadOnlyList<LegacyVocabularyToken> OptionalTokens { get; }
        public IReadOnlyList<LegacyVocabularyToken> CounterTokens { get; }
        public IReadOnlyList<string> LegacyFacts { get; }
        public IReadOnlyList<string> SourceReferences { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class MapRuleNormalizationSource
    {
        public MapRuleNormalizationSource(string id, string displayName, IEnumerable<LegacyVocabularyToken> publicTokens,
            IEnumerable<string> enemyProfileIds, IEnumerable<string> bossProfileIds, IEnumerable<string> legacyFacts,
            bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
        {
            Id = R.Text(id); DisplayName = R.Text(displayName); PublicTokens = R.Freeze(publicTokens);
            EnemyProfileIds = R.Strings(enemyProfileIds); BossProfileIds = R.Strings(bossProfileIds);
            LegacyFacts = R.Strings(legacyFacts); DevOnly = devOnly; IsEnabled = isEnabled; EntersFormalFlow = entersFormalFlow;
        }
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<LegacyVocabularyToken> PublicTokens { get; }
        public IReadOnlyList<string> EnemyProfileIds { get; }
        public IReadOnlyList<string> BossProfileIds { get; }
        public IReadOnlyList<string> LegacyFacts { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class CarrierMechanicBindingSource
    {
        public CarrierMechanicBindingSource(ValidationCarrierKind kind, string carrierId, string profileId, string reason)
        { Kind = kind; CarrierId = R.Text(carrierId); ProfileId = R.Text(profileId); Reason = R.Text(reason); }
        public ValidationCarrierKind Kind { get; }
        public string CarrierId { get; }
        public string ProfileId { get; }
        public string Reason { get; }
    }

    public sealed class NormalizationExceptionSource
    {
        public NormalizationExceptionSource(NormalizationExceptionKind kind, string sourceId, string status, string reason)
        { Kind = kind; SourceId = R.Text(sourceId); Status = R.Text(status); Reason = R.Text(reason); }
        public NormalizationExceptionKind Kind { get; }
        public string SourceId { get; }
        public string Status { get; }
        public string Reason { get; }
    }

    public sealed class EnemyValidationNormalizationInput
    {
        public EnemyValidationNormalizationInput(IEnumerable<CarrierNormalizationSource> carriers,
            IEnumerable<MechanicProfileNormalizationSource> profiles, IEnumerable<MapRuleNormalizationSource> mapRules,
            IEnumerable<CarrierMechanicBindingSource> carrierBindings, IEnumerable<NormalizationExceptionSource> exceptions)
        { Carriers = R.Freeze(carriers); Profiles = R.Freeze(profiles); MapRules = R.Freeze(mapRules); CarrierBindings = R.Freeze(carrierBindings); Exceptions = R.Freeze(exceptions); }
        public IReadOnlyList<CarrierNormalizationSource> Carriers { get; }
        public IReadOnlyList<MechanicProfileNormalizationSource> Profiles { get; }
        public IReadOnlyList<MapRuleNormalizationSource> MapRules { get; }
        public IReadOnlyList<CarrierMechanicBindingSource> CarrierBindings { get; }
        public IReadOnlyList<NormalizationExceptionSource> Exceptions { get; }
    }

    public sealed class EnemyValidationPlayerProjection
    {
        public EnemyValidationPlayerProjection(string id, string displayName, IEnumerable<string> mechanics,
            IEnumerable<string> pressures, IEnumerable<string> hints, IEnumerable<string> counterWindows)
        { Id = R.Text(id); DisplayName = R.Text(displayName); MechanicKeys = R.Set(mechanics); PressureKeys = R.Set(pressures); PlayerHintCategoryKeys = R.Set(hints); CounterWindowTypeKeys = R.Set(counterWindows); }
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> MechanicKeys { get; }
        public IReadOnlyList<string> PressureKeys { get; }
        public IReadOnlyList<string> PlayerHintCategoryKeys { get; }
        public IReadOnlyList<string> CounterWindowTypeKeys { get; }
    }

    public sealed class LegacyMappingTrace
    {
        public LegacyMappingTrace(string sourceKind, string key, string status, IEnumerable<string> targets, string reason)
        { SourceKind = R.Text(sourceKind); LegacyKey = R.Text(key); Status = R.Text(status); TargetKeys = R.Set(targets); Reason = R.Text(reason); }
        public string SourceKind { get; }
        public string LegacyKey { get; }
        public string Status { get; }
        public IReadOnlyList<string> TargetKeys { get; }
        public string Reason { get; }
    }

    public sealed class EnemyValidationDeveloperProjection
    {
        public EnemyValidationDeveloperProjection(string sourceKind, string sourceId, IEnumerable<string> required,
            IEnumerable<string> optional, IEnumerable<string> legacyFacts, IEnumerable<string> sourceReferences,
            IEnumerable<LegacyMappingTrace> trace, string bindingReason = "")
        { LegacySourceKind = R.Text(sourceKind); LegacySourceId = R.Text(sourceId); RequiredCapabilityKeys = R.Set(required); OptionalCapabilityKeys = R.Set(optional); LegacyFacts = R.Strings(legacyFacts); SourceReferenceIds = R.Set(sourceReferences); MappingTrace = R.Freeze(trace); BindingReason = R.Text(bindingReason); }
        public string LegacySourceKind { get; }
        public string LegacySourceId { get; }
        public IReadOnlyList<string> RequiredCapabilityKeys { get; }
        public IReadOnlyList<string> OptionalCapabilityKeys { get; }
        public IReadOnlyList<string> LegacyFacts { get; }
        public IReadOnlyList<string> SourceReferenceIds { get; }
        public IReadOnlyList<LegacyMappingTrace> MappingTrace { get; }
        public string BindingReason { get; }
        public bool DeveloperOnly => true;
    }

    public sealed class NormalizedEnemyCarrierSnapshot
    {
        public NormalizedEnemyCarrierSnapshot(EnemyArchetypeSnapshot domain, EnemyValidationPlayerProjection player, EnemyValidationDeveloperProjection developer)
        { Domain = domain; PlayerSafe = player; DeveloperOnly = developer; }
        public EnemyArchetypeSnapshot Domain { get; }
        public EnemyValidationPlayerProjection PlayerSafe { get; }
        public EnemyValidationDeveloperProjection DeveloperOnly { get; }
    }
    public sealed class NormalizedBossCarrierSnapshot
    {
        public NormalizedBossCarrierSnapshot(BossArchetypeSnapshot domain, EnemyValidationPlayerProjection player, EnemyValidationDeveloperProjection developer)
        { Domain = domain; PlayerSafe = player; DeveloperOnly = developer; }
        public BossArchetypeSnapshot Domain { get; }
        public EnemyValidationPlayerProjection PlayerSafe { get; }
        public EnemyValidationDeveloperProjection DeveloperOnly { get; }
    }
    public sealed class NormalizedMechanicProfileSnapshot
    {
        public NormalizedMechanicProfileSnapshot(ValidationProfileKind kind, MechanicProfileReference domain, EnemyValidationPlayerProjection player, EnemyValidationDeveloperProjection developer)
        { Kind = kind; Domain = domain; PlayerSafe = player; DeveloperOnly = developer; }
        public ValidationProfileKind Kind { get; }
        public MechanicProfileReference Domain { get; }
        public EnemyValidationPlayerProjection PlayerSafe { get; }
        public EnemyValidationDeveloperProjection DeveloperOnly { get; }
    }
    public sealed class NormalizedMapRuleSnapshot
    {
        public NormalizedMapRuleSnapshot(MapRuleReference domain, EnemyValidationPlayerProjection player, EnemyValidationDeveloperProjection developer)
        { Domain = domain; PlayerSafe = player; DeveloperOnly = developer; }
        public MapRuleReference Domain { get; }
        public EnemyValidationPlayerProjection PlayerSafe { get; }
        public EnemyValidationDeveloperProjection DeveloperOnly { get; }
    }
    public sealed class CarrierMechanicBindingSnapshot
    {
        public CarrierMechanicBindingSnapshot(ValidationCarrierKind kind, string carrierId, string profileId, string reason)
        { Kind = kind; CarrierId = R.Text(carrierId); MechanicProfileId = R.Text(profileId); Reason = R.Text(reason); }
        public ValidationCarrierKind Kind { get; }
        public string CarrierId { get; }
        public string MechanicProfileId { get; }
        public string Reason { get; }
        public string Status => "BOUND";
    }
    public sealed class MapRuleMechanicBindingSnapshot
    {
        public MapRuleMechanicBindingSnapshot(string mapRuleId, ValidationProfileKind kind, string profileId, string reason)
        { MapRuleId = R.Text(mapRuleId); Kind = kind; MechanicProfileId = R.Text(profileId); Reason = R.Text(reason); }
        public string MapRuleId { get; }
        public ValidationProfileKind Kind { get; }
        public string MechanicProfileId { get; }
        public string Reason { get; }
        public string Status => "BOUND";
    }
    public sealed class NormalizationExceptionSnapshot
    {
        public NormalizationExceptionSnapshot(NormalizationExceptionKind kind, string id, string status, string reason)
        { Kind = kind; SourceId = R.Text(id); Status = R.Text(status); Reason = R.Text(reason); }
        public NormalizationExceptionKind Kind { get; }
        public string SourceId { get; }
        public string Status { get; }
        public string Reason { get; }
        public bool DeveloperOnly => true;
    }

    public interface IEnemyValidationContentLookup
    {
        bool TryGetEnemy(string id, out NormalizedEnemyCarrierSnapshot value);
        bool TryGetBoss(string id, out NormalizedBossCarrierSnapshot value);
        bool TryGetMechanicProfile(string id, out NormalizedMechanicProfileSnapshot value);
        bool TryGetMapRule(string id, out NormalizedMapRuleSnapshot value);
    }

    public sealed class EnemyValidationContentSnapshot : IEnemyValidationContentLookup
    {
        private readonly IReadOnlyDictionary<string, NormalizedEnemyCarrierSnapshot> enemyById;
        private readonly IReadOnlyDictionary<string, NormalizedBossCarrierSnapshot> bossById;
        private readonly IReadOnlyDictionary<string, NormalizedMechanicProfileSnapshot> profileById;
        private readonly IReadOnlyDictionary<string, NormalizedMapRuleSnapshot> mapById;

        internal EnemyValidationContentSnapshot(IEnumerable<NormalizedEnemyCarrierSnapshot> enemies, IEnumerable<NormalizedBossCarrierSnapshot> bosses,
            IEnumerable<NormalizedMechanicProfileSnapshot> profiles, IEnumerable<NormalizedMapRuleSnapshot> maps,
            IEnumerable<CarrierMechanicBindingSnapshot> carrierBindings, IEnumerable<MapRuleMechanicBindingSnapshot> mapBindings,
            IEnumerable<NormalizationExceptionSnapshot> exceptions, EnemyDomainSnapshot domain)
        {
            Enemies = R.Freeze(enemies); Bosses = R.Freeze(bosses); MechanicProfiles = R.Freeze(profiles); MapRules = R.Freeze(maps);
            CarrierMechanicBindings = R.Freeze(carrierBindings); MapRuleMechanicBindings = R.Freeze(mapBindings); Exceptions = R.Freeze(exceptions);
            DomainSnapshot = domain; enemyById = Enemies.ToDictionary(x => x.Domain.StableId, StringComparer.Ordinal);
            bossById = Bosses.ToDictionary(x => x.Domain.StableId, StringComparer.Ordinal);
            profileById = MechanicProfiles.ToDictionary(x => x.Domain.StableId, StringComparer.Ordinal);
            mapById = MapRules.ToDictionary(x => x.Domain.StableId, StringComparer.Ordinal);
            CanonicalSignature = Canonical.Hash(BuildPayload(false));
            PlayerSafeCanonicalSignature = Canonical.Hash(BuildPayload(true));
        }
        public string SchemaId => EnemyValidationNormalizationSchema.SchemaId;
        public int SchemaVersion => EnemyValidationNormalizationSchema.SchemaVersion;
        public IReadOnlyList<NormalizedEnemyCarrierSnapshot> Enemies { get; }
        public IReadOnlyList<NormalizedBossCarrierSnapshot> Bosses { get; }
        public IReadOnlyList<NormalizedMechanicProfileSnapshot> MechanicProfiles { get; }
        public IReadOnlyList<NormalizedMapRuleSnapshot> MapRules { get; }
        public IReadOnlyList<CarrierMechanicBindingSnapshot> CarrierMechanicBindings { get; }
        public IReadOnlyList<MapRuleMechanicBindingSnapshot> MapRuleMechanicBindings { get; }
        public IReadOnlyList<NormalizationExceptionSnapshot> Exceptions { get; }
        public EnemyDomainSnapshot DomainSnapshot { get; }
        public string CanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }
        public bool TryGetEnemy(string id, out NormalizedEnemyCarrierSnapshot value) { if (id != null) return enemyById.TryGetValue(id, out value); value = null; return false; }
        public bool TryGetBoss(string id, out NormalizedBossCarrierSnapshot value) { if (id != null) return bossById.TryGetValue(id, out value); value = null; return false; }
        public bool TryGetMechanicProfile(string id, out NormalizedMechanicProfileSnapshot value) { if (id != null) return profileById.TryGetValue(id, out value); value = null; return false; }
        public bool TryGetMapRule(string id, out NormalizedMapRuleSnapshot value) { if (id != null) return mapById.TryGetValue(id, out value); value = null; return false; }

        private string BuildPayload(bool playerOnly)
        {
            StringBuilder b = new StringBuilder(32768); Canonical.Field(b, "schema", SchemaId); Canonical.Field(b, "version", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Canonical.Rows(b, "enemy", Enemies.Select(x => Canonical.Player(x.PlayerSafe) + (playerOnly ? "" : Canonical.Developer(x.DeveloperOnly))));
            Canonical.Rows(b, "boss", Bosses.Select(x => Canonical.Player(x.PlayerSafe) + (playerOnly ? "" : Canonical.Developer(x.DeveloperOnly))));
            Canonical.Rows(b, "profiles", MechanicProfiles.Select(x => ((int)x.Kind) + "|" + Canonical.Player(x.PlayerSafe) + (playerOnly ? "" : Canonical.Developer(x.DeveloperOnly))));
            Canonical.Rows(b, "maps", MapRules.Select(x => Canonical.Player(x.PlayerSafe) + (playerOnly ? "" : Canonical.Developer(x.DeveloperOnly))));
            if (!playerOnly) {
                Canonical.Rows(b, "carrierBindings", CarrierMechanicBindings.Select(x => ((int)x.Kind) + "|" + x.CarrierId + "|" + x.MechanicProfileId + "|" + x.Reason));
                Canonical.Rows(b, "mapBindings", MapRuleMechanicBindings.Select(x => x.MapRuleId + "|" + ((int)x.Kind) + "|" + x.MechanicProfileId));
                Canonical.Rows(b, "exceptions", Exceptions.Select(x => ((int)x.Kind) + "|" + x.SourceId + "|" + x.Status + "|" + x.Reason));
                Canonical.Field(b, "domain", DomainSnapshot.BuildCanonicalSignature());
            }
            return b.ToString();
        }
    }

    internal static class R
    {
        public static string Text(string value) => value ?? string.Empty;
        public static ReadOnlyCollection<string> Strings(IEnumerable<string> values) => Array.AsReadOnly((values ?? Array.Empty<string>()).Select(Text).ToArray());
        public static ReadOnlyCollection<string> Set(IEnumerable<string> values) => Array.AsReadOnly((values ?? Array.Empty<string>()).Select(Text).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray());
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    internal static class Canonical
    {
        public static string Player(EnemyValidationPlayerProjection x) => x.Id + "|" + x.DisplayName + "|" + string.Join(";", x.MechanicKeys) + "|" + string.Join(";", x.PressureKeys) + "|" + string.Join(";", x.PlayerHintCategoryKeys) + "|" + string.Join(";", x.CounterWindowTypeKeys);
        public static string Developer(EnemyValidationDeveloperProjection x) => "|DEV|" + x.LegacySourceKind + "|" + x.LegacySourceId + "|" + string.Join(";", x.RequiredCapabilityKeys) + "|" + string.Join(";", x.OptionalCapabilityKeys) + "|" + string.Join(";", x.LegacyFacts) + "|" + string.Join(";", x.SourceReferenceIds) + "|" + string.Join(";", x.MappingTrace.Select(t => t.SourceKind + ":" + t.LegacyKey + ":" + t.Status + ":" + string.Join("+", t.TargetKeys)));
        public static void Rows(StringBuilder b, string name, IEnumerable<string> rows) { string[] a = (rows ?? Array.Empty<string>()).OrderBy(x => x, StringComparer.Ordinal).ToArray(); Field(b, name + ".count", a.Length.ToString(CultureInfo.InvariantCulture)); for (int i = 0; i < a.Length; i++) Field(b, name + "[" + i + "]", a[i]); }
        public static void Field(StringBuilder b, string name, string value) { name = name ?? ""; value = value ?? ""; b.Append(name.Length).Append(':').Append(name).Append('=').Append(value.Length).Append(':').Append(value).Append(';'); }
        public static string Hash(string payload) { using (SHA256 sha = SHA256.Create()) { byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? "")); return "sha256:" + string.Concat(bytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture))); } }
    }
}
