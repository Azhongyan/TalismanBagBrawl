using System;
using System.Collections.Generic;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.CoreLoopLab
{
    public sealed class CoreLoopLabEncounterProfile
    {
        private readonly BattleSandboxExplicitDevEnemyActionCue[] cadence;

        public CoreLoopLabEncounterProfile(
            string profileId,
            BattleSandboxExplicitDevEncounterKind encounterKind,
            int playerMaxHp,
            int playerInitialShield,
            int enemyMaxHp,
            int basicAttackDamage,
            int skillDamage,
            int targetDurationMilliseconds,
            int basicAttackIntervalMilliseconds,
            int skillCastDurationMilliseconds,
            IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue> cadence,
            string enemyVisualProfileKey)
        {
            ProfileId = profileId ?? string.Empty;
            EncounterKind = encounterKind;
            PlayerMaxHp = playerMaxHp;
            PlayerInitialShield = playerInitialShield;
            EnemyMaxHp = enemyMaxHp;
            BasicAttackDamage = basicAttackDamage;
            SkillDamage = skillDamage;
            TargetDurationMilliseconds = targetDurationMilliseconds;
            BasicAttackIntervalMilliseconds = basicAttackIntervalMilliseconds;
            SkillCastDurationMilliseconds = skillCastDurationMilliseconds;
            this.cadence = cadence == null
                ? Array.Empty<BattleSandboxExplicitDevEnemyActionCue>()
                : new List<BattleSandboxExplicitDevEnemyActionCue>(cadence)
                    .ToArray();
            EnemyVisualProfileKey = enemyVisualProfileKey ?? string.Empty;
        }

        public string ProfileId { get; }
        public BattleSandboxExplicitDevEncounterKind EncounterKind { get; }
        public int PlayerMaxHp { get; }
        public int PlayerInitialShield { get; }
        public int EnemyMaxHp { get; }
        public int BasicAttackDamage { get; }
        public int SkillDamage { get; }
        public int TargetDurationMilliseconds { get; }
        public int BasicAttackIntervalMilliseconds { get; }
        public int SkillCastDurationMilliseconds { get; }
        public IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue> Cadence =>
            cadence;
        public string EnemyVisualProfileKey { get; }

        public BattleSandboxExplicitDevEncounterRequest BuildRequest(
            int generation,
            string startToken)
        {
            BattleSandboxExplicitDevEncounterRequest draft = CreateRequest(
                generation,
                startToken,
                string.Empty);
            return CreateRequest(
                generation,
                startToken,
                BattleSandboxExplicitDevEncounterContract
                    .ComputeProfileFingerprint(draft));
        }

        private BattleSandboxExplicitDevEncounterRequest CreateRequest(
            int generation,
            string startToken,
            string fingerprint)
        {
            return new BattleSandboxExplicitDevEncounterRequest(
                BattleSandboxExplicitDevEncounterRequest.CurrentSchemaId,
                true,
                BattleSandboxExplicitDevEncounterRequest.RequiredProductContext,
                BattleSandboxExplicitDevEncounterRequest.ApprovedHostContext,
                BattleSandboxExplicitDevEncounterRequest.ApprovedHostPackageId,
                false,
                false,
                false,
                ProfileId,
                generation,
                startToken,
                CoreLoopLabEncounterProfiles.EnemyIdentityD13,
                EncounterKind,
                PlayerMaxHp,
                PlayerInitialShield,
                EnemyMaxHp,
                BasicAttackDamage,
                SkillDamage,
                TargetDurationMilliseconds,
                BasicAttackIntervalMilliseconds,
                SkillCastDurationMilliseconds,
                cadence,
                fingerprint);
        }
    }

    public static class CoreLoopLabEncounterProfiles
    {
        public const string EnemyIdentityD13 = "d1_3";
        public const string D13VisualProfileKey = "d1_3.v1";

        public static readonly CoreLoopLabEncounterProfile Normal = new(
            "core_loop_lab.d1_3.normal.v1",
            BattleSandboxExplicitDevEncounterKind.Normal,
            999,
            180,
            360,
            34,
            68,
            18000,
            4500,
            800,
            new[]
            {
                new BattleSandboxExplicitDevEnemyActionCue(
                    1,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    4000),
                new BattleSandboxExplicitDevEnemyActionCue(
                    2,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    8500),
                new BattleSandboxExplicitDevEnemyActionCue(
                    3,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    13000)
            },
            D13VisualProfileKey);

        public static readonly CoreLoopLabEncounterProfile Elite = new(
            "core_loop_lab.d1_3.elite.v1",
            BattleSandboxExplicitDevEncounterKind.Elite,
            999,
            180,
            800,
            42,
            86,
            30000,
            4400,
            1100,
            new[]
            {
                new BattleSandboxExplicitDevEnemyActionCue(
                    1,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    3500),
                new BattleSandboxExplicitDevEnemyActionCue(
                    2,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    7000),
                new BattleSandboxExplicitDevEnemyActionCue(
                    3,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    11000),
                new BattleSandboxExplicitDevEnemyActionCue(
                    4,
                    BattleSandboxExplicitDevEnemyActionKind.Skill,
                    14000),
                new BattleSandboxExplicitDevEnemyActionCue(
                    5,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    16500),
                new BattleSandboxExplicitDevEnemyActionCue(
                    6,
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack,
                    22000)
            },
            D13VisualProfileKey);

        public static CoreLoopLabEncounterProfile ForBattleIndex(
            int battleIndex)
        {
            return battleIndex == 1 ? Normal : Elite;
        }
    }
}
