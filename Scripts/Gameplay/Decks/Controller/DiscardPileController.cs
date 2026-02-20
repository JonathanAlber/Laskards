using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Interaction;
using Gameplay.Decks.View;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Services;
using UnityEngine;
using Utility;
using Utility.Logging;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Controller for the discard pile, which holds played cards.
    /// </summary>
    public sealed class DiscardPileController : DeckController<CardDefinition, CardController, DiscardPileView>
    {
        [Header("View")]
        [Tooltip("Visual component for this discard pile.")]
        [SerializeField] private DiscardPileView discardView;

        protected override DiscardPileView View => discardView;

        protected override EDeck ZoneTypeInternal => EDeck.Discard;

        private bool _isRecycling;
        private bool _isReceivingCardFromHand;

        protected override void Awake()
        {
            base.Awake();

            GameFlowSystem.OnPhaseStarted += OnPhaseStarted;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFlowSystem.OnPhaseStarted -= OnPhaseStarted;
        }

        /// <summary>
        /// Returns all cards in this discard pile to their original owners with animation.
        /// </summary>
        public void RecycleToOriginDecks()
        {
            CustomLogger.Log("Starting recycle of discard pile to origin decks.", this);

            if (_isRecycling)
            {
                CustomLogger.Log("Recycle already in progress. Skipping new recycle request.", this);
                return;
            }

            if (Model.Count == 0)
            {
                CustomLogger.Log("Discard pile is empty. No cards to recycle.", this);
                return;
            }

            _isRecycling = true;

            Dictionary<CardController, Vector3> startPositions = new(Model.Count);
            for (int i = 0; i < Model.Count; i++)
            {
                CardController card = Model.Cards[i];
                if (card == null)
                {
                    CustomLogger.LogWarning("Null card found in discard pile during recycle. Skipping.", this);
                    continue;
                }

                startPositions[card] = card.transform.position;
            }

            List<CardController> removed = Model.RemoveAll();
            removed.Reverse();

            List<CardController> toAnimate = new();
            Dictionary<ICardOwner, List<CardController>> byOwner = new();
            Dictionary<ICardOwner, int> remainingByOwner = new();

            foreach (CardController card in removed)
            {
                if (card == null)
                {
                    CustomLogger.LogWarning("Null card found in removed cards during recycle. Skipping.", this);
                    continue;
                }

                if (card.InitialOwner == null)
                {
                    CustomLogger.LogWarning($"Card {card.name} has no initial owner. Recycling skipped.", this);
                    continue;
                }

                ICardOwner owner = card.InitialOwner;

                if (!byOwner.TryGetValue(owner, out List<CardController> list))
                {
                    list = new List<CardController>();
                    byOwner[owner] = list;
                    remainingByOwner[owner] = 0;
                }

                list.Add(card);
                remainingByOwner[owner]++;

                owner.AddCard(card);
                toAnimate.Add(card);
            }

            if (remainingByOwner.Count == 0 || toAnimate.Count == 0)
            {
                CustomLogger.Log("No valid cards or owners found for recycle. Ending recycle process.", this);
                _isRecycling = false;
                return;
            }

            int ownersRemaining = remainingByOwner.Count;

            discardView.AnimateRecycleToOwners(toAnimate, startPositions, onCardArrived: card =>
            {
                if (card == null)
                {
                    CustomLogger.LogWarning("Null card encountered during recycle animation callback..", this);
                    return;
                }

                if (card.InitialOwner == null)
                {
                    CustomLogger.LogWarning($"Card {card.name} has no initial owner during recycle animation " +
                                            "callback.", this);
                    return;
                }

                ICardOwner owner = card.InitialOwner;

                if (!remainingByOwner.TryGetValue(owner, out int left))
                {
                    CustomLogger.LogWarning("No remaining count found for owner during recycle animation " +
                                            $"callback for card {card.name}.", this);
                    return;
                }

                left--;
                remainingByOwner[owner] = left;

                if (left > 0)
                    return;

                ownersRemaining--;

                if (owner is IDiscardRecycleReceiver receiver && byOwner.TryGetValue(owner, out List<CardController> _))
                    receiver.OnDiscardRecycleCompleted();

                if (ownersRemaining <= 0)
                    _isRecycling = false;
            });
        }

        protected override void OnCardAdded(CardController card)
        {
            base.OnCardAdded(card);

            if (_isReceivingCardFromHand)
                return;

            if (card.InitialOwner is { CardCount: 0 })
                RecycleToOriginDecks();
        }

        private void CheckIfInitialOwnersEmpty()
        {
            int count = Model.Count;
            for (int i = 0; i < count; i++)
            {
                CardController card = Model.Cards[i];
                if (card.InitialOwner == null)
                {
                    CustomLogger.LogWarning($"Card {card.name} in discard pile has no initial owner.", this);
                    continue;
                }

                if (card.InitialOwner.CardCount > 0)
                    continue;

                RecycleToOriginDecks();
                break;
            }
        }

        private void OnPhaseStarted(GameState state)
        {
            if (state.CurrentPhase == EGamePhase.PlayerMove)
                MoveHandToDiscard();
        }

        private void MoveHandToDiscard()
        {
            if (!ServiceLocator.TryGet(out HandDeckController hand))
                return;

            List<CardController> cards = hand.RemoveAll();
            if (cards.Count == 0)
                return;

            if (!ServiceLocator.TryGet(out CardTweenController tweens))
            {
                foreach (CardController card in cards)
                    Add(card);

                return;
            }

            _isReceivingCardFromHand = cards.Count > 0;
            int tweenedCount = 0;
            foreach (CardController card in cards)
            {
                Add(card);
                tweens.TweenCardTo(card, Vector3.zero, Quaternion.identity, BaseScale,
                    onComplete: () =>
                {
                    tweenedCount++;
                    if (tweenedCount < cards.Count)
                        return;

                    _isReceivingCardFromHand = false;
                    CoroutineRunner.Instance.RunNextFrame(CheckIfInitialOwnersEmpty);
                });
            }
        }
    }
}