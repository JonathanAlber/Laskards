using Gameplay.Decks.Zones;
using Interaction;
using UnityEngine;

namespace Gameplay.Cards.Interaction
{
    /// <summary>
    /// Interaction gate for a card, controlling when it can be interacted with.
    /// </summary>
    public sealed class CardGate : MonoBehaviour, IInteractionGate
    {
        /// <summary>
        /// True while the card is transitioning (e.g., during a tween or move animation).
        /// </summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>
        /// True, while the card is being dragged by the player.
        /// </summary>
        public bool IsBeingDragged { get; private set; }

        public bool AllowDragging => _zone is { AllowDragging: true };

        private ICardZone _zone;

        /// <summary>
        /// Sets the zone that owns this card.
        /// </summary>
        /// <param name="zone"></param>
        public void SetZone(ICardZone zone) => _zone = zone;

        /// <summary>
        /// Disables interaction during transition animations.
        /// </summary>
        public void StartTransition() => IsTransitioning = true;

        /// <summary>
        /// Re-enables interaction after transitions complete.
        /// </summary>
        public void FinishTransition() => IsTransitioning = false;

        /// <summary>
        /// Sets whether this card is currently being dragged.
        /// </summary>
        /// <param name="isDragging"><c>true</c> if the card is being dragged; otherwise, <c>false</c>.</param>
        public void SetDragging(bool isDragging) => IsBeingDragged = isDragging;
    }
}