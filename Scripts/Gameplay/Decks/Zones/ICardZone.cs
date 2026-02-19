using Gameplay.Cards;
using Gameplay.Cards.Data;
using UnityEngine;

namespace Gameplay.Decks.Zones
{
    /// <summary>
    /// Runtime zone that currently "owns" card instances (Hand, ActionDeck, UnitDeck, Discard, etc.).
    /// Provides a decoupled interaction surface for hover / drag / play logic so input code does not
    /// need to know concrete deck implementations.
    /// </summary>
    public interface ICardZone
    {
        /// <summary>
        /// Logical zone type for this group of cards (e.g. Hand, ActionDeck, Discard).
        /// </summary>
        EDeck ZoneType { get; }

        /// <summary>
        /// Resting scale cards should have while they are idling in this zone.
        /// Used by hover / tween logic so visual systems do not have to guess layout scale.
        /// </summary>
        Vector3 BaseScale { get; }

        /// <summary>
        /// Returns if cards in this zone can be dragged by the player.
        /// </summary>
        bool AllowDragging { get; }

        /// <summary>
        /// Returns true if this zone currently considers the given card to be its top/front card.
        /// </summary>
        /// <param name="card">Card to evaluate.</param>
        bool IsTopCard(CardController card);

        /// <summary>
        /// Called when a card in this zone is hovered by the pointer.
        /// </summary>
        /// <param name="card">The card being hovered.</param>
        void OnHoverEnter(CardController card);

        /// <summary>
        /// Called when a card in this zone stops being hovered.
        /// </summary>
        /// <param name="card">The card that is no longer hovered.</param>
        void OnHoverExit(CardController card);

        /// <summary>
        /// Called when a drag on one of this zone's cards starts.
        /// </summary>
        /// <param name="card">The card whose drag started.</param>
        void OnDragStarted(CardController card);

        /// <summary>
        /// Called when a drag on one of this zone's cards ends but the play was not accepted.
        /// The zone should restore the card (and itself) to its resting layout.
        /// </summary>
        /// <param name="card">The card whose drag ended.</param>
        void ReturnCard(CardController card);

        /// <summary>
        /// Called when a card from this zone was successfully played.
        /// </summary>
        /// <param name="card">The card that was played.</param>
        void OnCardPlayed(CardController card);

        /// <summary>
        /// Attempts to accept a play for a given card that currently belongs to this zone.
        /// Returns true if the play was executed (even if follow-up movement/visuals could not complete).
        /// </summary>
        /// <param name="card">The card to attempt to play.</param>
        /// <param name="failReason">If the play fails, contains the reason; otherwise, null.</param>
        /// <returns>True if the play was executed; otherwise, false.</returns>
        bool TryAcceptPlay(CardController card, out string failReason);
    }
}