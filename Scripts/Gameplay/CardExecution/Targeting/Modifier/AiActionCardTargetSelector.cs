using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Movement.AI;

namespace Gameplay.CardExecution.Targeting.Modifier
{
    /// <summary>
    /// Base class for AI target selectors for action card modifiers.
    /// </summary>
    public abstract class AiActionCardTargetSelector
    {
        /// <summary>
        /// How deep the AI should search when selecting targets.
        /// </summary>
        protected readonly int SearchDepth;

        protected readonly BossMinimaxSearch Minimax;

        protected AiActionCardTargetSelector(int searchDepth, BossMinimaxSearch minimax)
        {
            SearchDepth = searchDepth;
            Minimax = minimax;
        }

        /// <summary>
        /// Attempts to select a target for the given action card model.
        /// </summary>
        /// <param name="actionCardModel">The action card model for which to select a target.</param>
        /// <param name="target">The selected target, if any.</param>
        /// <returns><c>true</c> if a target was successfully selected; otherwise, <c>false</c>.</returns>
        public abstract bool TrySelectTarget(ActionCardModel actionCardModel, out IModifiableBase target);
    }
}