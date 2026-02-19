using System;
using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Decks.Controller;
using Gameplay.Cards.Data;
using Systems.Services;
using UnityEngine;
using Utility.Logging;
// ReSharper disable UnusedMember.Local

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Cheat commands for drawing cards from the drawable decks.
    /// </summary>
    public class DeckDrawCheats : MonoBehaviour
    {
        [CheatCommand("draw_action", Description = "Draws <amount> cards from the Action Deck.",
            Usage = "draw_action <amount>")]
        private void DrawAction(int amount)
        {
            if (!ServiceLocator.TryGet(out ActionCardDeckController action))
                return;

            if (!action.TryDraw(amount))
                CustomLogger.Log("Action deck has no cards to draw.", null);
        }

        [CheatCommand("recycle_discard",
            Description = "Recycles all cards from the discard pile back to their origin decks.",
            Usage = "recycle_discard")]
        private void RecycleDiscardPile()
        {
            if (ServiceLocator.TryGet(out DiscardPileController discardPile))
                discardPile.RecycleToOriginDecks();
        }

        [CheatCommand("draw_action_category",
            Description = "Draws <amount> cards from the Action Deck filtered by category.",
            Usage = "draw_action_category <category> <amount>\nCategories: Buff, Ressource, Field, Unit, Player")]
        private void DrawActionCategory(string categoryString, int amount)
        {
            if (!ServiceLocator.TryGet(out ActionCardDeckController action))
                return;

            if (!Enum.TryParse(categoryString, true, out EActionCategory category))
            {
                CustomLogger.Log($"Invalid category '{categoryString}'.", null);
                return;
            }

            if (!action.TryDrawByCategory(category, amount, out List<CardController> _))
                CustomLogger.Log($"Did not find any cards of category {category}.", null);
        }

        [CheatCommand("draw_raven", Description = "Draws <amount> Ravens from the Unit-Deck.",
            Usage = "draw_raven <amount>")]
        private void DrawUnits(int amount)
        {
            if (!ServiceLocator.TryGet(out UnitCardDeckController unit))
                return;

            if (!unit.TryDraw(amount))
                CustomLogger.Log("Raven deck has no cards to draw.", null);
        }
    }
}