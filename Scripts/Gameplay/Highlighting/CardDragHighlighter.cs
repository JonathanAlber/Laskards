using UnityEngine;
using Gameplay.Cards;
using UnityEngine.UI;
using Utility.Logging;
using Systems.Services;
using Gameplay.Decks.Controller;
using System.Collections.Generic;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Base class for handling visualization for dragging cards,
    /// such as highlighting valid targets.
    /// </summary>
    public abstract class CardDragHighlighter : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("GraphicRaycaster used for UI tile raycasts while dragging.")]
        [SerializeField] protected GraphicRaycaster uiRaycaster;

        private readonly List<CardController> _hookedCards = new();

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet(out HandDeckController handDeckController))
                return;

            handDeckController.Model.OnCardCountChanged += HandleHandCardCountChanged;
        }

        private void OnDisable()
        {
            UnhookAllCards();

            if (!ServiceLocator.TryGet(out HandDeckController handDeckController))
                return;

            handDeckController.Model.OnCardCountChanged -= HandleHandCardCountChanged;
        }

        /// <summary>
        /// Called when dragging of a card starts.
        /// </summary>
        protected abstract void HandleDragStarted(CardController card);

        /// <summary>
        /// Called while dragging a card.
        /// </summary>
        protected abstract void HandleDragging(CardController card);

        /// <summary>
        /// Called when dragging of a card ends.
        /// </summary>
        protected abstract void HandleDragEnded(CardController card);

        /// <summary>
        /// Validates whether the given card should be hooked for drag events.
        /// </summary>
        protected abstract bool ValidateCard(CardController card);

        private void HandleHandCardCountChanged(IReadOnlyList<CardController> cards)
        {
            UnhookAllCards();
            HookAllCards();
        }

        private void HookAllCards()
        {
            if (!ServiceLocator.TryGet(out HandDeckController hand))
                return;

            _hookedCards.Clear();

            IReadOnlyList<CardController> cards = hand.Model.Cards;
            foreach (CardController card in cards)
            {
                if (card == null)
                {
                    CustomLogger.LogWarning("Attempted to hook a null card.", this);
                    continue;
                }

                if (!ValidateCard(card))
                    continue;

                card.CardInteractionAdapter.OnDragStarted += HandleDragStarted;
                card.CardInteractionAdapter.OnDragging += HandleDragging;
                card.CardInteractionAdapter.OnDragEnded += HandleDragEnded;

                _hookedCards.Add(card);
            }
        }

        private void UnhookAllCards()
        {
            if (_hookedCards.Count == 0)
                return;

            foreach (CardController card in _hookedCards)
            {
                if (card == null)
                {
                    CustomLogger.LogWarning("Attempted to unhook a null card.", this);
                    continue;
                }

                card.CardInteractionAdapter.OnDragStarted -= HandleDragStarted;
                card.CardInteractionAdapter.OnDragging -= HandleDragging;
                card.CardInteractionAdapter.OnDragEnded -= HandleDragEnded;
            }

            _hookedCards.Clear();
        }
    }
}