using Gameplay.CardExecution.Targeting;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Interaction;
using Gameplay.Decks.View;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Player;
using Systems.Services;
using UnityEngine;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Controller for the player's hand of cards.
    /// </summary>
    public sealed class HandDeckController : DeckController<CardDefinition, CardController, HandDeckView>
    {
        [Header("View")]
        [Tooltip("Visual component for the hand.")]
        [SerializeField] private HandDeckView handView;

        public override bool AllowDragging => _allowDragging;

        protected override EDeck ZoneTypeInternal => EDeck.Hand;

        protected override HandDeckView View => handView;

        private bool _allowDragging = true;

        protected override void Awake()
        {
            base.Awake();

            GameFlowSystem.OnPhaseStarted += OnGamePhaseStarted;
            GameFlowSystem.OnPhaseEnded += OnGamePhaseEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFlowSystem.OnPhaseStarted -= OnGamePhaseStarted;
            GameFlowSystem.OnPhaseEnded -= OnGamePhaseEnded;
        }

        public override void OnHoverEnter(CardController card) => handView?.Focus(card);

        public override void OnHoverExit(CardController card) => handView?.Unfocus(card);

        public override void OnDragStarted(CardController card)
        {
            if (!ServiceLocator.TryGet(out CardTweenController tween))
                return;

            tween.RotateCardTo(card, Quaternion.identity, markCardTransitioning: false);
            handView?.Focus(card);
        }

        public override void ReturnCard(CardController card) => handView?.Unfocus(card);

        public override void OnCardPlayed(CardController card) { }

        public override bool TryAcceptPlay(CardController card, out string failReason)
        {
            failReason = string.Empty;

            if (GameFlowSystem.CurrentPhase != EGamePhase.PlayerPlay)
            {
                failReason = "Cards can only be played during the Player Play phase.";
                return false;
            }

            if (!ServiceLocator.TryGet(out PlayerCardTargetResolver resolver))
            {
                failReason = "No target resolver found.";
                return false;
            }

            if (!ServiceLocator.TryGet(out PlayerController player))
            {
                failReason = "No card player found.";
                return false;
            }

            if (!card.TryPlay(player, resolver, out failReason))
                return false;

            Remove(card);
            OnCardPlayed(card);
            return true;
        }

        private void OnGamePhaseStarted(GameState gameState)
        {
            _allowDragging = gameState.CurrentPhase == EGamePhase.PlayerPlay;
        }

        private void OnGamePhaseEnded(GameState gameState) => _allowDragging = false;
    }
}