using System.Collections.Generic;
using System.Linq;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Decks.View;
using Gameplay.Flow;
using Gameplay.StarterDecks.Data;
using Systems.ObjectPooling.Gameplay;
using Utility.Logging;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Controller for the action deck, which the player can draw from.
    /// It contains both action cards and unit cards.
    /// </summary>
    public sealed class ActionCardDeckController : DrawableDeckController<CardDefinition, CardController, DrawableDeckView>
    {
        protected override EDeck ZoneTypeInternal => EDeck.ActionDeck;

        protected override void Awake()
        {
            base.Awake();

            GameContextService.OnStarterDeckChosen += Initialize;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameContextService.OnStarterDeckChosen -= Initialize;
        }

        /// <summary>
        /// Draws up to '<paramref name="amount"/>' cards from this deck that match the given category.
        /// </summary>
        public bool TryDrawByCategory(EActionCategory category, int amount, out List<CardController> drawnCards)
        {
            drawnCards = DrawCardsByCategory(category, amount);
            if (drawnCards.Count == 0)
                return false;

            DrawToHand(drawnCards);
            return true;
        }

        protected override void HandleCardClicked(CardController card) => DrawToHandValidated(card);

        protected override CardController GetCardInstance(CardDefinition def)
        {
            switch (def)
            {
                case ActionCardDefinition:
                    return ActionCardPool.Instance.Get();
                case UnitCardDefinition:
                    return UnitCardPool.Instance.Get();
                default:
                    CustomLogger.LogWarning($"Unsupported card type: {def.GetType().Name}", this);
                    return null;
            }
        }

        private void Initialize(StarterDeckDefinition starterDeck) => InitializeFromDeckData(starterDeck.Cards);

        private List<CardController> DrawCardsByCategory(EActionCategory category, int amount)
        {
            List<CardController> matchingCards = new();

            if (category == EActionCategory.Unit)
                matchingCards.AddRange(Model.Cards.OfType<UnitCardController>().Take(amount));

            amount -= matchingCards.Count;
            if (amount <= 0)
                return matchingCards;

            matchingCards.AddRange(Model.Cards.OfType<ActionCardController>()
                .Where(c => c.TypedModel.ModifierState.ActionCategory == category).Take(amount));

            if (matchingCards.Count == 0)
                CustomLogger.LogWarning($"No cards found in category {category}.", this);

            return matchingCards;
        }
    }
}