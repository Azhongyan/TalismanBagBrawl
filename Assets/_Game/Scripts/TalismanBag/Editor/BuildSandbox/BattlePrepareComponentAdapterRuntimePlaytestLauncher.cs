#if UNITY_EDITOR
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    [InitializeOnLoad]
    public static class BattlePrepareComponentAdapterRuntimePlaytestLauncher
    {
        private const string PendingSessionKey =
            "TalismanBag.BuildSandbox.BattlePrepareComponentAdapterRuntimePlaytest01.PendingLaunch";

        static BattlePrepareComponentAdapterRuntimePlaytestLauncher()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        public static void OpenRuntimePlaytest()
        {
            if (EditorApplication.isPlaying)
            {
                InstallRuntimePlaytestNow();
                return;
            }

            SessionState.SetBool(PendingSessionKey, true);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                SessionState.EraseBool(PendingSessionKey);
                Debug.Log("[BattlePrepareRuntimePlaytest01] Launch cancelled before opening the target scene.");
                return;
            }

            EditorSceneManager.OpenScene(
                BattlePrepareComponentAdapterRuntimePlaytestPlan.TargetScenePath,
                OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        public static void InstallRuntimePlaytestNow()
        {
            BattlePrepareComponentAdapterRuntimePlaytestController controller =
                BattlePrepareComponentAdapterRuntimePlaytestController.InstallRuntimePlaytest();
            Debug.Log(
                $"[BattlePrepareRuntimePlaytest01] Runtime launcher installed. blocked={controller.Blocked}, maturePrepareOpened={controller.MaturePrepareOpened}.");
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode
                || !SessionState.GetBool(PendingSessionKey, false))
            {
                return;
            }

            SessionState.EraseBool(PendingSessionKey);
            EditorApplication.delayCall += InstallAfterPlayModeEntered;
        }

        private static void InstallAfterPlayModeEntered()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            InstallRuntimePlaytestNow();
        }
    }
}
#endif
