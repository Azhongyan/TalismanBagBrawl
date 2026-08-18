using System;
using System.Linq;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1FormalItemTrayView : MonoBehaviour
    {
        [SerializeField] private RectTransform trayRect;
        [SerializeField] private C1FormalItemCardView[] cardViews;

        public void AssignForEditor(
            RectTransform configuredTrayRect,
            C1FormalItemCardView[] configuredCardViews)
        {
            trayRect = configuredTrayRect;
            cardViews = configuredCardViews ?? Array.Empty<C1FormalItemCardView>();
        }

        public bool ValidateAuthoredReferences()
        {
            return trayRect != null && cardViews != null && cardViews.Length == 3
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
            C1FormalItemRosterEntrySnapshot[] trayRows = snapshot == null
                ? Array.Empty<C1FormalItemRosterEntrySnapshot>()
                : snapshot.roster
                    .Where(row => snapshot.FindPlacementByInstanceId(
                        row.itemInstanceId) == null)
                    .OrderBy(row => row.itemInstanceId, StringComparer.Ordinal)
                    .ToArray();
            for (int index = 0; index < (cardViews?.Length ?? 0); index++)
            {
                C1FormalItemCardView card = cardViews[index];
                if (card == null) continue;
                if (index >= trayRows.Length)
                {
                    card.Clear();
                    continue;
                }

                card.Bind(trayRows[index], null, null);
            }
        }

        internal bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            return trayRect != null && RectTransformUtility.RectangleContainsScreenPoint(
                trayRect,
                screenPoint,
                eventCamera);
        }
    }
}
