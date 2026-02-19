using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Decks.Data;
using Gameplay.Decks.View;
using Systems.ObjectPooling.Gameplay;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Decks.Controller
{
    /// <summary>
    /// Controller for the unit card deck, which the player can draw from.
    /// </summary>
    public sealed class UnitCardDeckController : DrawableDeckController<UnitCardDefinition, UnitCardController,
        DrawableDeckView>
    {
        [Header("Unit Deck Settings")]

        [Tooltip("The unit deck definition to initialize the unit deck with.")]
        [SerializeField] private UnitDeckData deckDefinition;

        protected override EDeck ZoneTypeInternal => EDeck.UnitDeck;

        private void Start() => InitializeFromDefinition();

        private void InitializeFromDefinition()
        {
            if (deckDefinition == null || deckDefinition.BaseUnitCard == null)
            {
                CustomLogger.LogWarning("Unit deck definition or base unit card is null. " +
                                        "Cannot initialize unit deck.", this);
                return;
            }

            if (deckDefinition.MaxUnits <= 0)
            {
                CustomLogger.LogWarning("Unit deck definition has non-positive max units. " +
                                        "Cannot initialize unit deck.", this);
                return;
            }

            List<UnitCardDefinition> unitCards = new();
            for (int i = 0; i < deckDefinition.MaxUnits; i++)
                unitCards.Add((UnitCardDefinition)deckDefinition.BaseUnitCard);

            InitializeFromDeckData(unitCards);
        }

        protected override void HandleCardClicked(CardController card)
        {
            if (card is UnitCardController unitCard)
                DrawToHandValidated(unitCard);
        }

        protected override UnitCardController GetCardInstance(UnitCardDefinition data) => UnitCardPool.Instance.Get();
    }
}