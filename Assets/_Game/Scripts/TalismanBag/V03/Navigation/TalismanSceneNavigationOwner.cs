using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.Navigation
{
    public enum TalismanSceneRoute
    {
        BootEntry = 0,
        MainHome = 1,
        WorldMap = 2,
        TalismanUpgrade = 3,
        UnifiedBattle = 5
    }

    public static class TalismanSceneNavigationOwner
    {
        public const string BootEntrySceneName = "Scene_TalismanBag_V03_BootEntry";
        public const string BootEntryScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity";
        public const string MainHomeSceneName = "Scene_TalismanBag_V03_MainHome";
        public const string MainHomeScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";
        public const string WorldMapSceneName = "Scene_TalismanBag_V04_WorldMap";
        public const string WorldMapScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity";
        public const string TalismanUpgradeSceneName =
            "Scene_TalismanBag_V03_TalismanUpgrade";
        public const string TalismanUpgradeScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_TalismanUpgrade.unity";
        public const string UnifiedBattleSceneName =
            "Scene_TalismanBag_V04_UnifiedBattlePageShell";
        public const string UnifiedBattleScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";

        private static bool navigationPending;
        private static UpgradeReturnContext upgradeReturnContext;

        public static bool IsNavigationPending => navigationPending;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            navigationPending = false;
            upgradeReturnContext = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BindSceneLoaded()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        public static bool TryNavigate(
            TalismanSceneRoute destination,
            UnityEngine.Object logContext = null)
        {
            return TryNavigateInternal(destination, null, logContext);
        }

        public static bool TryNavigateToUpgrade(
            TalismanSceneRoute returnRoute,
            UnityEngine.Object logContext = null)
        {
            if (returnRoute == TalismanSceneRoute.TalismanUpgrade)
            {
                Debug.LogError(
                    "[MainSceneNavigation] Upgrade return route cannot target Upgrade itself.",
                    logContext);
                return false;
            }

            UpgradeReturnContext context =
                new UpgradeReturnContext(returnRoute);
            return TryNavigateInternal(
                TalismanSceneRoute.TalismanUpgrade,
                () => upgradeReturnContext = context,
                logContext);
        }

        public static bool TryNavigateBackFromUpgrade(
            TalismanSceneRoute fallbackRoute,
            UnityEngine.Object logContext = null)
        {
            TalismanSceneRoute destination =
                upgradeReturnContext != null
                    ? upgradeReturnContext.ReturnRoute
                    : fallbackRoute;

            bool accepted = TryNavigateInternal(
                destination,
                () => upgradeReturnContext = null,
                logContext);
            return accepted;
        }

        public static bool IsAvailableInPlayer(TalismanSceneRoute route)
        {
            return SceneUtility.GetBuildIndexByScenePath(GetScenePath(route)) >= 0;
        }

        public static string GetSceneName(TalismanSceneRoute route)
        {
            return route switch
            {
                TalismanSceneRoute.BootEntry => BootEntrySceneName,
                TalismanSceneRoute.MainHome => MainHomeSceneName,
                TalismanSceneRoute.WorldMap => WorldMapSceneName,
                TalismanSceneRoute.TalismanUpgrade => TalismanUpgradeSceneName,
                TalismanSceneRoute.UnifiedBattle => UnifiedBattleSceneName,
                _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
            };
        }

        public static string GetScenePath(TalismanSceneRoute route)
        {
            return route switch
            {
                TalismanSceneRoute.BootEntry => BootEntryScenePath,
                TalismanSceneRoute.MainHome => MainHomeScenePath,
                TalismanSceneRoute.WorldMap => WorldMapScenePath,
                TalismanSceneRoute.TalismanUpgrade => TalismanUpgradeScenePath,
                TalismanSceneRoute.UnifiedBattle => UnifiedBattleScenePath,
                _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
            };
        }

        private static bool TryNavigateInternal(
            TalismanSceneRoute destination,
            Action onAccepted,
            UnityEngine.Object logContext)
        {
            if (navigationPending)
            {
                Debug.LogWarning(
                    "[MainSceneNavigation] Navigation request ignored while another request is pending.",
                    logContext);
                return false;
            }

            string scenePath = GetScenePath(destination);
            string sceneName = GetSceneName(destination);
            if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
            {
                Debug.LogError(
                    $"[MainSceneNavigation] Scene is missing from Build Settings: {scenePath}",
                    logContext);
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() &&
                string.Equals(activeScene.name, sceneName, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"[MainSceneNavigation] Navigation request ignored because {sceneName} is already active.",
                    logContext);
                return false;
            }

            navigationPending = true;
            try
            {
                AsyncOperation operation =
                    SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (operation == null)
                {
                    navigationPending = false;
                    Debug.LogError(
                        $"[MainSceneNavigation] LoadSceneAsync returned null for {sceneName}.",
                        logContext);
                    return false;
                }

                onAccepted?.Invoke();
                if (destination != TalismanSceneRoute.TalismanUpgrade &&
                    onAccepted == null)
                {
                    upgradeReturnContext = null;
                }

                operation.completed += HandleNavigationCompleted;
                return true;
            }
            catch (Exception exception)
            {
                navigationPending = false;
                Debug.LogException(exception, logContext);
                return false;
            }
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            navigationPending = false;
        }

        private static void HandleNavigationCompleted(AsyncOperation operation)
        {
            navigationPending = false;
        }

        private sealed class UpgradeReturnContext
        {
            public UpgradeReturnContext(TalismanSceneRoute returnRoute)
            {
                ReturnRoute = returnRoute;
            }

            public TalismanSceneRoute ReturnRoute { get; }
        }
    }
}
