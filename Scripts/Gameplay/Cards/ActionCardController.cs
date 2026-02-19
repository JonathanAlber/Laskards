using System;
using Gameplay.CardExecution;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Cards.View;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Cards
{
    /// <summary>
    /// Represents an action card that can be played to apply effects in the game.
    /// </summary>
    public sealed class ActionCardController : CardController
    {
        /// <summary>
        /// Invoked when an action card starts being dragged.
        /// </summary>
        public static event Action<ActionCardModel> OnDragStarted;

        /// <summary>
        /// Invoked when an action card ends being dragged.
        /// </summary>
        public static event Action<ActionCardModel> OnDragEnded;

        [Header("Action Card")]
        [Tooltip("The typed definition for this action card.")]
        [SerializeField] private ActionCardView cardView;

        /// <summary>
        /// Strongly-typed runtime model for this action card.
        /// </summary>
        public ActionCardModel TypedModel { get; private set; }

        public override CardView View => cardView;

        public override CardModel Model => TypedModel;

        private void Awake()
        {
            if (CardInteractionAdapter == null)
                return;

            CardInteractionAdapter.OnDragStarted += HandleDragStarted;
            CardInteractionAdapter.OnDragEnded += HandleDragEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            TypedModel?.OnCleanup();

            if (CardInteractionAdapter == null)
                return;

            CardInteractionAdapter.OnDragStarted -= HandleDragStarted;
            CardInteractionAdapter.OnDragEnded -= HandleDragEnded;
        }

        public override void Initialize(CardDefinition cardDefinition)
        {
            if (cardDefinition is ActionCardDefinition typed)
            {
                Initialize(typed);
            }
            else
            {
                CustomLogger.LogWarning($"Tried to initialize {nameof(ActionCardController)} with " +
                                        $"non-{nameof(ActionCardDefinition)}.", this);
            }
        }

        public override bool TryPlay(ICardPlayer player, ICardTargetResolver resolver, out string failReason)
        {
            if (!ActionCardExecutor.TryExecute(TypedModel, player, resolver, out failReason))
                return false;

            player.OnCardExecuted(this);

            NotifyCardPlayed();

            return true;
        }

        /// <summary>
        /// Typed initialization for action cards.
        /// </summary>
        private void Initialize(ActionCardDefinition definition)
        {
            TypedModel = new ActionCardModel(definition);
            cardView.Initialize(TypedModel, TypedModel.ModifierState.ActionCategory);
        }

        private void HandleDragStarted(CardController _) => OnDragStarted?.Invoke(TypedModel);

        private void HandleDragEnded(CardController _) => OnDragEnded?.Invoke(TypedModel);
    }
}