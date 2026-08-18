using System;
using System.Globalization;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1FormalItemArrangementPresenter : MonoBehaviour
    {
        public const string AuthoredReferenceMissingDiagnostic =
            "C1_FORMAL_ITEM_PRESENTATION_AUTHORED_REFERENCE_MISSING";

        [SerializeField] private C1FormalItemBoardView boardView;
        [SerializeField] private C1FormalItemTrayView trayView;

        private C1FormalItemSessionAuthority authority;
        private long commandSequence;
        private bool missingReferenceLogged;

        public bool IsBound => authority != null;
        public C1FormalItemSessionSnapshot Current => authority?.Current;

        public void AssignForEditor(
            C1FormalItemBoardView configuredBoardView,
            C1FormalItemTrayView configuredTrayView)
        {
            boardView = configuredBoardView;
            trayView = configuredTrayView;
        }

        public bool ValidateAuthoredReferences()
        {
            return boardView != null && trayView != null
                   && boardView.ValidateAuthoredReferences()
                   && trayView.ValidateAuthoredReferences();
        }

        public bool Bind(
            C1FormalItemSessionAuthority configuredAuthority,
            out string diagnostic)
        {
            if (configuredAuthority == null || !ValidateAuthoredReferences())
            {
                diagnostic = AuthoredReferenceMissingDiagnostic;
                LogMissingOnce();
                return false;
            }

            if (ReferenceEquals(authority, configuredAuthority))
            {
                PublishCurrent();
                diagnostic = C1FormalItemSessionDiagnosticCodes.None;
                return true;
            }

            Unbind();
            authority = configuredAuthority;
            commandSequence = 0;
            boardView.BindPresenter(this);
            trayView.BindPresenter(this);
            PublishCurrent();
            diagnostic = C1FormalItemSessionDiagnosticCodes.None;
            return true;
        }

        public void Unbind()
        {
            if (boardView != null) boardView.BindPresenter(null);
            if (trayView != null) trayView.BindPresenter(null);
            authority = null;
            commandSequence = 0;
        }

        public void PublishCurrent()
        {
            if (authority == null || !ValidateAuthoredReferences())
            {
                LogMissingOnce();
                return;
            }

            boardView.Bind(authority.Current);
            trayView.Bind(authority.Current);
        }

        public bool TryHandleCardDrop(
            C1FormalItemCardView card,
            Vector2 screenPoint,
            Camera eventCamera)
        {
            if (authority == null || card == null || !ValidateAuthoredReferences())
            {
                LogMissingOnce();
                return false;
            }

            if (boardView.TryScreenPointToCell(
                    screenPoint,
                    eventCamera,
                    out Vector2Int cell))
            {
                C1FormalItemArrangementCommandKind kind = card.IsPlaced
                    ? C1FormalItemArrangementCommandKind.MoveOnBoard
                    : C1FormalItemArrangementCommandKind.PlaceFromTray;
                return SubmitExactlyOne(new C1FormalItemArrangementCommand(
                    kind,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    NextCommandId(kind),
                    card.ItemInstanceId,
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(cell, card.Rotation)));
            }

            if (trayView.ContainsScreenPoint(screenPoint, eventCamera)
                && card.IsPlaced)
            {
                return SubmitExactlyOne(new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    NextCommandId(C1FormalItemArrangementCommandKind.ReturnToTray),
                    card.ItemInstanceId,
                    authority.Current.canonicalSignature,
                    null));
            }

            PublishCurrent();
            return false;
        }

        private bool SubmitExactlyOne(C1FormalItemArrangementCommand command)
        {
            C1FormalItemSessionOperationResult result = authority.Submit(command);
            PublishCurrent();
            return result != null && result.accepted;
        }

        private string NextCommandId(C1FormalItemArrangementCommandKind kind)
        {
            commandSequence++;
            return string.Join(".", new[]
            {
                "c1formal-presenter",
                authority.Current.sessionToken,
                authority.Current.resetGeneration.ToString(CultureInfo.InvariantCulture),
                ((int)kind).ToString(CultureInfo.InvariantCulture),
                commandSequence.ToString(CultureInfo.InvariantCulture)
            });
        }

        private void LogMissingOnce()
        {
            if (missingReferenceLogged) return;
            missingReferenceLogged = true;
            Debug.LogError("[C1FormalItemPresenter] "
                           + AuthoredReferenceMissingDiagnostic, this);
        }

        private void OnDisable()
        {
            Unbind();
        }
    }
}
