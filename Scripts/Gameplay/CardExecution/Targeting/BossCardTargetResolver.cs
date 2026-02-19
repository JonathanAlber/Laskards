using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;
using Utility.Logging;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Simple resolver that delegates boss action targeting to AI selectors
    /// and places boss units on the back row using a minimax evaluation.
    /// </summary>
    public sealed class BossCardTargetResolver : ICardTargetResolver
    {
        private readonly AiUnitCardResolver _unitResolver;
        private readonly AiActionCardResolver _actionResolver;

        public BossCardTargetResolver(int searchDepth, BossMinimaxSearch minimax)
        {
            _unitResolver = new AiUnitCardResolver(searchDepth, minimax);
            _actionResolver = new AiActionCardResolver(searchDepth, minimax);
        }

        /// <summary>
        /// Attempts to resolve a target for the given card model.
        /// </summary>
        public bool TryResolveTarget(CardModel cardModel, out IModifiableBase target)
        {
            switch (cardModel)
            {
                case ActionCardModel actionCardModel:
                {
                    return _actionResolver.TryResolveTarget(actionCardModel, out target);
                }
                case UnitCardModel unitCardModel:
                {
                    return _unitResolver.TryResolveTarget(unitCardModel, out target);
                }
                default:
                {
                    CustomLogger.LogWarning("Unsupported card type received in boss resolver.", null);
                    target = null;
                    return false;
                }
            }
        }
    }
}