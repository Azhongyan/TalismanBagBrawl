using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.UnifiedBattle
{
    [DisallowMultipleComponent]
    public sealed class UnifiedBattleBattleLikePreviewAreaBridgeHost : MonoBehaviour
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool formalFlow;
        [SerializeField] private bool connectedToFormalRoute;
        [SerializeField] private bool writesSaveData;
        [SerializeField] private bool grantsReward;
        [SerializeField] private bool advancesChapter;
        [SerializeField] private RectTransform battleLikePreviewArea;
        [SerializeField] private BuildGridInteractionPreviewController previewController;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool FormalFlow => formalFlow;
        public bool ConnectedToFormalRoute => connectedToFormalRoute;
        public bool WritesSaveData => writesSaveData;
        public bool GrantsReward => grantsReward;
        public bool AdvancesChapter => advancesChapter;
        public RectTransform BattleLikePreviewArea => battleLikePreviewArea;
        public BuildGridInteractionPreviewController PreviewController => previewController;

        public string BuildStatusText()
        {
            string areaState = battleLikePreviewArea == null ? "missing" : "bound";
            string controllerState = previewController == null ? "missing" : "bound";
            return "BattleLikePreviewArea bridge: " +
                   areaState +
                   ", controller: " +
                   controllerState +
                   ", devOnly=true, formalFlow=false, save=false, reward=false, chapter=false.";
        }

        public void AssignForEditor(
            RectTransform area,
            BuildGridInteractionPreviewController controller)
        {
            battleLikePreviewArea = area;
            previewController = controller;
        }
    }
}
