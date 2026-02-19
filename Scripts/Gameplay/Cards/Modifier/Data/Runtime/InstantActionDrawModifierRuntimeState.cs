using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Decks.Controller;
using Gameplay.Player;
using Gameplay.StatLayers.Cards;
using Systems.Services;
using Utility.Logging;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="InstantActionDrawModifierData"/>.
    /// Draws cards of a specific action category, optionally making them free
    /// until they are played by applying a temporary common stat layer.
    /// </summary>
    public sealed class InstantActionDrawModifierRuntimeState
        : TypedModifierRuntimeState<InstantActionDrawModifierData, PlayerController>
    {
        private readonly EActionCategory _actionCategory;
        private readonly int _cardsToDraw;
        private readonly bool _makeDrawnCardsFree;

        private readonly Dictionary<CardController, FreeUntilPlayedLayer> _activeLayers;

        /// <summary>
        /// Initializes a new runtime state of this modifier.
        /// </summary>
        public InstantActionDrawModifierRuntimeState(InstantActionDrawModifierData data) : base(data)
        {
            _actionCategory = data.ActionCategoryToDraw;
            _cardsToDraw = data.CardsToDraw;
            _makeDrawnCardsFree = data.MakeCardsFree;

            _activeLayers = new Dictionary<CardController, FreeUntilPlayedLayer>();
        }

        /// <summary>
        /// Executes the draw effect and applies temporary free-cost layers if configured.
        /// </summary>
        public override void Execute()
        {
            if (!ServiceLocator.TryGet(out ActionCardDeckController deckController))
                return;

            if (!deckController.TryDrawByCategory(_actionCategory, _cardsToDraw, out List<CardController> drawnCards))
            {
                CustomLogger.LogWarning("Failed to draw cards from Action Deck.", null);
                return;
            }

            if (!_makeDrawnCardsFree)
                return;

            foreach (CardController card in drawnCards)
            {
                if (card.Model is not IStatLayerHost<ICommonCardStatLayer> host)
                    continue;

                FreeUntilPlayedLayer layer = new();
                host.AddLayer(layer);
                _activeLayers[card] = layer;

                card.OnPlayed += HandleCard;
                card.OnDestroyed += HandleCard;
            }
        }

        /// <summary>
        /// Creates a typed effect instance for the target player.
        /// </summary>
        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new PlayerEffect(target, creatorTeam, DurationType, Duration, EffectData);
        }

        /// <summary>
        /// Handles the moment when a card affected by this modifier is played or destroyed.
        /// </summary>
        private void HandleCard(CardController card)
        {
            if (!_activeLayers.TryGetValue(card, out FreeUntilPlayedLayer layer))
                return;

            if (card.Model is IStatLayerHost<ICommonCardStatLayer> host)
                host.RemoveLayer(layer);

            _activeLayers.Remove(card);
            card.OnPlayed -= HandleCard;
            card.OnDestroyed -= HandleCard;
        }
    }
}