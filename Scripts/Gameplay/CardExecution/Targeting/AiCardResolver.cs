using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Interface for resolving targets for AI-controlled card plays, based on the card type.
    /// </summary>
    public abstract class AiCardResolver<T> where T : CardModel
    {
        /// <summary>
        /// How deep the AI should search when resolving targets.
        /// </summary>
        protected readonly int SearchDepth;

        /// <summary>
        /// Minimax search instance for evaluating potential targets.
        /// </summary>
        protected readonly BossMinimaxSearch Minimax;

        protected AiCardResolver(int searchDepth, BossMinimaxSearch minimax)
        {
            SearchDepth = searchDepth;
            Minimax = minimax;
        }

        /// <summary>
        /// Attempts to resolve a target for the given card model.
        /// </summary>
        /// <param name="cardModel">The card model for which to resolve a target.</param>
        /// <param name="target">The resolved target, if any.</param>
        /// <returns><c>true</c> if a target was successfully resolved; otherwise, <c>false</c>.</returns>
        public abstract bool TryResolveTarget(T cardModel, out IModifiableBase target);
    }
}