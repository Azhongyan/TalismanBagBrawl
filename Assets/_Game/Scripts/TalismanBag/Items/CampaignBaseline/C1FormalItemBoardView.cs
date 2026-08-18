using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1FormalItemBoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform boardRect;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private Image[] cells;
        [SerializeField] private C1FormalItemCardView[] cardViews;

        public void AssignForEditor(
            RectTransform configuredBoardRect,
            RectTransform configuredGridRoot,
            Image[] configuredCells,
            C1FormalItemCardView[] configuredCardViews)
        {
            boardRect = configuredBoardRect;
            gridRoot = configuredGridRoot;
            cells = configuredCells ?? Array.Empty<Image>();
            cardViews = configuredCardViews ?? Array.Empty<C1FormalItemCardView>();
        }

        public bool ValidateAuthoredReferences()
        {
            return boardRect != null && gridRoot != null
                   && cells != null && cells.Length == 25
                   && cells.All(value => value != null)
                   && cells.Distinct().Count() == 25
                   && cardViews != null && cardViews.Length == 3
                   && cardViews.All(value => value != null
                       && value.ValidateAuthoredReferences());
        }

        internal void BindPresenter(C1FormalItemArrangementPresenter presenter)
        {
            foreach (C1FormalItemCardView card in cardViews
                         ?? Array.Empty<C1FormalItemCardView>())
            {
                if (card != null) card.BindPresenter(presenter);
            }
        }

        internal void Bind(C1FormalItemSessionSnapshot snapshot)
        {
            C1FormalItemRosterEntrySnapshot[] placedRows = snapshot == null
                ? Array.Empty<C1FormalItemRosterEntrySnapshot>()
                : snapshot.roster
                    .Where(row => snapshot.FindPlacementByInstanceId(
                        row.itemInstanceId) != null)
                    .OrderBy(row => row.itemInstanceId, StringComparer.Ordinal)
                    .ToArray();
            for (int index = 0; index < (cardViews?.Length ?? 0); index++)
            {
                C1FormalItemCardView card = cardViews[index];
                if (card == null) continue;
                if (index >= placedRows.Length)
                {
                    card.Clear();
                    continue;
                }

                C1FormalItemRosterEntrySnapshot roster = placedRows[index];
                C1FormalItemPlacementSnapshot placement =
                    snapshot.FindPlacementByInstanceId(roster.itemInstanceId);
                ItemSystemPlacementSnapshot resolved =
                    snapshot.itemSystemSnapshot.FindPlacement(placement.placementId);
                PositionCard(card, resolved);
                card.Bind(roster, placement, resolved);
            }
        }

        internal bool TryScreenPointToCell(
            Vector2 screenPoint,
            Camera eventCamera,
            out Vector2Int cell)
        {
            cell = default;
            if (gridRoot == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    gridRoot,
                    screenPoint,
                    eventCamera,
                    out Vector2 local))
            {
                return false;
            }

            Rect rect = gridRoot.rect;
            if (!rect.Contains(local) || rect.width <= 0f || rect.height <= 0f)
            {
                return false;
            }

            int x = Mathf.FloorToInt((local.x - rect.xMin) / rect.width * 5f);
            int y = Mathf.FloorToInt((local.y - rect.yMin) / rect.height * 5f);
            if (x < 0 || x >= 5 || y < 0 || y >= 5) return false;
            cell = new Vector2Int(x, y);
            return true;
        }

        private void PositionCard(
            C1FormalItemCardView card,
            ItemSystemPlacementSnapshot placement)
        {
            if (card == null || placement == null || placement.OccupiedCells.Count == 0)
                return;
            RectTransform rect = card.transform as RectTransform;
            if (rect == null) return;
            int minX = placement.OccupiedCells.Min(value => value.x);
            int maxX = placement.OccupiedCells.Max(value => value.x);
            int minY = placement.OccupiedCells.Min(value => value.y);
            int maxY = placement.OccupiedCells.Max(value => value.y);
            rect.anchorMin = new Vector2(minX / 5f, minY / 5f);
            rect.anchorMax = new Vector2((maxX + 1) / 5f, (maxY + 1) / 5f);
            rect.offsetMin = new Vector2(3f, 3f);
            rect.offsetMax = new Vector2(-3f, -3f);
        }
    }
}
