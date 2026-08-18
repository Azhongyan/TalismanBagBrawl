using UnityEngine;

namespace TalismanBag.UnifiedBattle
{
    public sealed class UnifiedBattlePageShellMarker : MonoBehaviour
    {
        public const string SceneName = "Scene_TalismanBag_V04_UnifiedBattlePageShell";
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        public const string PrefabPath = "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool formalFlow;
        [SerializeField] private bool connectedToFormalRoute;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool FormalFlow => formalFlow;
        public bool ConnectedToFormalRoute => connectedToFormalRoute;

        public void ConfigureFormalForEditor()
        {
            devOnly = false;
            isEnabled = true;
            formalFlow = true;
            connectedToFormalRoute = true;
        }
    }
}
