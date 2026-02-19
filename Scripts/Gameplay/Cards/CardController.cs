using System;
using Gameplay.CardExecution;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards.Data;
using Gameplay.Cards.Interaction;
using Gameplay.Cards.Model;
using Gameplay.Cards.View;
using UnityEngine;

namespace Gameplay.Cards
{
    /// <summary>
    /// Base class for all card types in the game.
    /// </summary>
    public abstract class CardController : MonoBehaviour
    {
        /// <summary>
        /// Invoked when a card is played.
        /// </summary>
        public static event Action OnCardPlayed;

        /// <summary>
        /// Invoked when the card is successfully played.
        /// </summary>
        public event Action<CardController> OnPlayed;

        /// <summary>
        /// Invoked when the card is destroyed.
        /// </summary>
        public event Action<CardController> OnDestroyed;

        [field: Header("Common Components")]
        [field: Tooltip("The gate that manages interactions for this card.")]
        [field: SerializeField] public CardGate CardGate { get; private set; }

        [field: Tooltip("The adapter that wires interaction events to this card.")]
        [field: SerializeField] public CardInteractionAdapter CardInteractionAdapter { get; private set; }

        [field: Tooltip("Handles front/back face visibility based on rotation.")]
        [field: SerializeField] public CardFlipper CardFlipper { get; set; }

        protected virtual void OnDestroy() => OnDestroyed?.Invoke(this);

        /// <summary>
        /// The owner of this card at initialization.
        /// </summary>
        public ICardOwner InitialOwner { get; private set; }

        /// <summary>
        /// Visual component for this card.
        /// </summary>
        public abstract CardView View { get; }

        /// <summary>
        /// Runtime model backing this instance.
        /// </summary>
        public abstract CardModel Model { get; }

        /// <summary>
        /// Sets the initial owner of this card.
        /// </summary>
        /// <param name="owner"></param>
        public void SetInitialOwner(ICardOwner owner) => InitialOwner = owner;

        /// <summary>
        /// Initializes the card with the provided definition.
        /// </summary>
        /// <param name="cardDefinition">The definition to initialize the card with.</param>
        public abstract void Initialize(CardDefinition cardDefinition);

        /// <summary>
        /// Attempts to play this card using the provided player and target resolver.
        /// </summary>
        /// <param name="player">The card player attempting to play the card.</param>
        /// <param name="resolver">The target resolver to determine valid targets.</param>
        /// <param name="failReason">The reason for failure, if playing was unsuccessful.</param>
        /// <returns><c>true</c> if the card was successfully played; otherwise, <c>false</c></returns>
        public abstract bool TryPlay(ICardPlayer player, ICardTargetResolver resolver, out string failReason);

        /// <summary>
        /// Notifies subscribers that this card has been played.
        /// </summary>
        protected void NotifyCardPlayed()
        {
            OnPlayed?.Invoke(this);
            OnCardPlayed?.Invoke();
        }
    }
}