using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Decks.Controller;
using Gameplay.Player;
using Systems.Services;
using Utility.Logging;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="InstantDrawModifierData"/>.
    /// Draws a specified number of cards from a specified deck immediately.
    /// </summary>
    public sealed class InstantDrawModifierRuntimeState
        : TypedModifierRuntimeState<InstantDrawModifierData, PlayerController>
    {
        private readonly EDeck _deck;
        private readonly int _cardsToDraw;

        public InstantDrawModifierRuntimeState(InstantDrawModifierData data) : base(data)
        {
            _cardsToDraw = data.CardsToDraw;
            _deck = data.Deck;
        }

        public override void Execute()
        {
            switch (_deck)
            {
                case EDeck.ActionDeck:
                {
                    if (ServiceLocator.TryGet(out ActionCardDeckController actionDeck) && !actionDeck.TryDraw(_cardsToDraw))
                        CustomLogger.LogWarning("Failed to draw cards from Action Deck.", null);
                    break;
                }
                case EDeck.UnitDeck:
                {
                    if (ServiceLocator.TryGet(out UnitCardDeckController unitDeck) && !unitDeck.TryDraw(_cardsToDraw))
                        CustomLogger.LogWarning("Failed to draw cards from Unit Deck.", null);
                    break;
                }
                case EDeck.None:
                case EDeck.Hand:
                case EDeck.Discard:
                default:
                    CustomLogger.LogWarning($"Unsupported deck type: {_deck}", null);
                    break;
            }
        }

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new PlayerEffect(target, creatorTeam, DurationType, Duration, EffectData);
        }
    }
}