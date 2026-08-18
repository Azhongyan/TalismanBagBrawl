using System;
using System.Collections.Generic;
using System.Reflection;
using TalismanBag.Contracts.Battle;
using TalismanBag.Presentation.FormalBattle;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattlePerActorEnemyPresentationVerifier
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PrivateStatic =
            BindingFlags.Static | BindingFlags.NonPublic;

        public static void ExecuteFromCommandLine()
        {
            try
            {
                Run();
                Debug.Log("PER_ACTOR_ENEMY_ACTION_DECOR_PASS checks=4");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Run()
        {
            GameObject rootObject = new GameObject("PerActorDecorVerifier");
            var created = new List<GameObject> { rootObject };
            try
            {
                FormalBattlePresentationRoot root =
                    rootObject.AddComponent<FormalBattlePresentationRoot>();
                FormalBattleEnemySlotView[] slots =
                    new FormalBattleEnemySlotView[3];
                for (int index = 0; index < slots.Length; index++)
                {
                    GameObject slotObject = new GameObject("Enemy" + index);
                    created.Add(slotObject);
                    slots[index] =
                        slotObject.AddComponent<FormalBattleEnemySlotView>();
                    SetField(slots[index], "authoredStableOrder", index);
                    SetField(slots[index], "actorBalanceId", "enemy-" + index);
                    SetField(slots[index], "defeated", false);
                }

                GameObject playerObject = new GameObject("Player");
                created.Add(playerObject);
                FormalBattlePlayerPresentationView player = playerObject
                    .AddComponent<FormalBattlePlayerPresentationView>();
                GameObject fxObject = new GameObject("CueFx");
                created.Add(fxObject);
                FormalBattleCueFxAudioRoot fx =
                    fxObject.AddComponent<FormalBattleCueFxAudioRoot>();

                SetField(root, "enemySlots", slots);
                SetField(root, "playerView", player);
                SetField(root, "cueFxAudioRoot", fx);

                for (int index = 0; index < slots.Length; index++)
                {
                    InvokeApplyCue(root, Cue(
                        "scheduled-" + index,
                        index + 1,
                        C1FormalRealtimeBattleCueKinds.EnemyAttackScheduled,
                        "enemy-" + index,
                        "attack-" + index));
                }
                Require(
                    slots[0].EditorTelegraphPlayCount == 1
                    && slots[1].EditorTelegraphPlayCount == 1
                    && slots[2].EditorTelegraphPlayCount == 1,
                    "PER_ACTOR_DECOR_TELEGRAPH_SOURCE_MISMATCH");

                InvokeApplyCue(root, Cue(
                    "accepted-0",
                    4,
                    C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                    "enemy-0",
                    "attack-0"));
                InvokeApplyCue(root, Cue(
                    "accepted-1",
                    5,
                    C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                    "enemy-1",
                    "attack-1"));
                Require(
                    slots[0].EditorAttackPlayCount == 1
                    && slots[1].EditorAttackPlayCount == 1
                    && slots[2].EditorAttackPlayCount == 0,
                    "PER_ACTOR_DECOR_ATTACK_SOURCE_MISMATCH");

                InvokeApplyCue(root, new C1FormalRealtimeBattleCue(
                    "defeated-2",
                    6,
                    1000L,
                    C1FormalRealtimeBattleCueKinds.ActorDefeated,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    "item-source",
                    "enemy-2",
                    2,
                    100,
                    100,
                    1,
                    0,
                    1,
                    1,
                    "-1",
                    10,
                    "defeat-2"));
                InvokeApplyCue(root, Cue(
                    "accepted-2",
                    7,
                    C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                    "enemy-2",
                    "attack-2"));
                Require(
                    slots[2].Defeated
                    && slots[2].EditorAttackPlayCount == 0,
                    "PER_ACTOR_DECOR_DEFEATED_ATTACK_PLAYED");

                C1FormalRealtimeBattleCue accepted = Cue(
                    "accepted-chain",
                    8,
                    C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                    "enemy-0",
                    "shared-event");
                C1FormalRealtimeBattleCue hit = Cue(
                    "hit-chain",
                    9,
                    C1FormalRealtimeBattleCueKinds.HitAccepted,
                    "enemy-0",
                    "shared-event");
                C1FormalRealtimeBattleCue damage = Cue(
                    "damage-chain",
                    10,
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                    "enemy-0",
                    "shared-event");
                string acceptedRole = BuildRoleKey(accepted);
                string hitRole = BuildRoleKey(hit);
                string damageRole = BuildRoleKey(damage);
                InvokeApplyCue(root, hit);
                float playerHitStartedAt = GetField<float>(
                    player,
                    "hitStartedAt");
                Require(
                    playerHitStartedAt > -100f
                    && !string.Equals(
                        acceptedRole,
                        hitRole,
                        StringComparison.Ordinal)
                    && !string.Equals(
                        hitRole,
                        damageRole,
                        StringComparison.Ordinal),
                    "PER_ACTOR_DECOR_APPLICATION_CHAIN_COLLAPSED");
            }
            finally
            {
                for (int index = created.Count - 1; index >= 0; index--)
                {
                    if (created[index] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(created[index]);
                    }
                }
            }
        }

        private static C1FormalRealtimeBattleCue Cue(
            string cueId,
            long sequence,
            string cueKind,
            string sourceId,
            string acceptedApplicationEventId)
        {
            return new C1FormalRealtimeBattleCue(
                cueId,
                sequence,
                1000L,
                cueKind,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                sourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                100,
                90,
                0,
                0,
                10,
                10,
                "-10",
                10,
                acceptedApplicationEventId);
        }

        private static void InvokeApplyCue(
            FormalBattlePresentationRoot root,
            C1FormalRealtimeBattleCue cue)
        {
            MethodInfo method = typeof(FormalBattlePresentationRoot)
                .GetMethod("ApplyCue", PrivateInstance);
            if (method == null)
            {
                throw new InvalidOperationException(
                    "Formal ApplyCue method is missing.");
            }
            method.Invoke(root, new object[] { cue });
        }

        private static string BuildRoleKey(C1FormalRealtimeBattleCue cue)
        {
            MethodInfo method = typeof(FormalBattlePresentationRoot)
                .GetMethod(
                    "BuildAcceptedApplicationRoleKey",
                    PrivateStatic);
            if (method == null)
            {
                throw new InvalidOperationException(
                    "Formal application role key method is missing.");
            }
            return (string)method.Invoke(null, new object[] { cue });
        }

        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                PrivateInstance);
            if (field == null)
            {
                throw new InvalidOperationException(
                    "Missing field " + fieldName + " on "
                    + target.GetType().Name);
            }
            field.SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                PrivateInstance);
            if (field == null)
            {
                throw new InvalidOperationException(
                    "Missing field " + fieldName + " on "
                    + target.GetType().Name);
            }
            return (T)field.GetValue(target);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }
    }
}
