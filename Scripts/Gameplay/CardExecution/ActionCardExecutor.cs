using Gameplay.CardExecution.Targeting;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;

namespace Gameplay.CardExecution
{
    /// <summary>
    /// Executes <see cref="ActionCardController"/> effects against provided targets.
    /// Decoupled from input and resource systems.
    /// </summary>
    public static class ActionCardExecutor
    {
        /// <summary>
        /// Attempts to execute the provided <see cref="ActionCardController"/>
        /// by applying its effect to a valid target.
        /// </summary>
        /// <param name = "actionCardModel">The action card model to execute.</param>
        /// <param name="player">The card player executing the card.</param>
        /// <param name="resolver">The target resolver to determine valid targets.</param>
        /// <param name="failReason">The reason for failure, if execution was unsuccessful.</param>
        /// <returns><c>true</c> if the card was successfully executed; otherwise, <c>false</c>.</returns>
        public static bool TryExecute(ActionCardModel actionCardModel, ICardPlayer player, ICardTargetResolver resolver,
            out string failReason)
        {
            failReason = null;

            if (actionCardModel == null)
            {
                failReason = "Invalid card model.";
                return false;
            }

            if (!actionCardModel.CanBePlayedBy(player, out failReason))
                return false;

            if (!player.CanPlay(actionCardModel))
            {
                failReason = "Cannot afford card.";
                return false;
            }

            if (!resolver.TryResolveTarget(actionCardModel, out IModifiableBase modifiable))
            {
                failReason = "No valid target.";
                return false;
            }

            if (!actionCardModel.ModifierState.Validate(modifiable))
            {
                failReason = "Target rejected card.";
                return false;
            }

            if (!modifiable.ValidatePlayer(player.Team))
            {
                failReason = "Target cannot be modified by card-player from team " + player.Team + ".";
                return false;
            }

            if (actionCardModel.ModifierState.TryCreateEffect(modifiable, player.Team, out IEffect effect)
                && actionCardModel.ModifierState.DurationType is EDurationType.Temporary or EDurationType.Permanent)
                modifiable.AddEffect(effect);

            actionCardModel.ModifierState.Execute();

            return true;
        }
    }
}