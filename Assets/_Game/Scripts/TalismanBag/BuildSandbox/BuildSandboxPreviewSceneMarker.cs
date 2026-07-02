using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildSandboxPreviewSceneMarker : MonoBehaviour
    {
        public const string SceneName = "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool connectedToFormalFlow;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ConnectedToFormalFlow => connectedToFormalFlow;
    }
}
