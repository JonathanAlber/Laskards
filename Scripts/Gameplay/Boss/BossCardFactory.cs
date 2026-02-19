using System.Collections.Generic;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Utility.Logging;

namespace Gameplay.Boss
{
    /// <summary>
    /// Provides creation of runtime card models for the boss from authored definitions.
    /// </summary>
    public static class BossCardFactory
    {
        /// <summary>
        /// Attempts to build card models from a set of card definitions.
        /// </summary>
        /// <param name="cardDefinitions">Definitions to convert.</param>
        /// <param name="cardModels">Created card models.</param>
        /// <returns><c>true</c> if at least one card model was created; otherwise <c>false</c>.</returns>
        public static bool TryCreate(IEnumerable<CardDefinition> cardDefinitions, out List<CardModel> cardModels)
        {
            cardModels = new List<CardModel>();

            if (cardDefinitions == null)
                return false;

            foreach (CardDefinition cardDefinition in cardDefinitions)
            {
                if (TryCreate(cardDefinition, out CardModel cardModel))
                    cardModels.Add(cardModel);
            }

            return cardModels.Count > 0;
        }
        /// <summary>
        /// Attempts to build a card model from a single definition.
        /// </summary>
        /// <param name="definition">Card definition to convert.</param>
        /// <param name="cardModel">Created card model.</param>
        /// <returns><c>true</c> if the card model was created; otherwise <c>false</c>.</returns>
        public static bool TryCreate(CardDefinition definition, out CardModel cardModel)
        {
            switch (definition)
            {
                case UnitCardDefinition unit:
                    cardModel = new UnitCardModel(unit);
                    return true;
                case ActionCardDefinition action:
                    cardModel = new ActionCardModel(action);
                    return true;
                default:
                    CustomLogger.LogError($"Unsupported card definition type: {definition.GetType()}", null);
                    cardModel = null;
                    return false;
            }
        }
    }
}