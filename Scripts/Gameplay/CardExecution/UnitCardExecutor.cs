using Gameplay.Board;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Gameplay.Units;
using Systems.ObjectPooling.Gameplay;
using Systems.Services;

namespace Gameplay.CardExecution
{
    /// <summary>
    /// Executes <see cref="UnitCardController"/> placement logic against provided tiles.
    /// Decoupled from input and resource systems.
    /// </summary>
    public static class UnitCardExecutor
    {
        /// <summary>
        /// Attempts to execute the provided <see cref="UnitCardController"/> by
        /// placing its <see cref="UnitController"/> on a valid <see cref="Tile"/>.
        /// </summary>
        /// <param name="cardModel">The unit card model to execute.</param>
        /// <param name="player">The player attempting to play the card.</param>
        /// <param name="resolver">The target resolver to find a valid tile.</param>
        /// <param name="failReason">Output parameter for the reason of failure, if any.</param>
        /// <param name = "newUnit">The newly placed unit, if successful.</param>
        /// <returns><c>true</c> if the card was successfully played; otherwise, <c>false</c>.</returns>
        public static bool TryExecute(UnitCardModel cardModel, ICardPlayer player, ICardTargetResolver resolver,
            out string failReason, out UnitController newUnit)
        {
            failReason = null;
            newUnit = null;

            if (cardModel == null)
            {
                failReason = "Invalid card.";
                return false;
            }

            if (!cardModel.CanBePlayedBy(player, out failReason))
                return false;

            if (!player.CanPlay(cardModel))
            {
                failReason = "Cannot afford card.";
                return false;
            }

            if (!resolver.TryResolveTarget(cardModel, out IModifiableBase target))
            {
                failReason = "No valid target.";
                return false;
            }

            Tile tile = target as Tile;
            if (tile == null)
            {
                failReason = "Invalid tile target.";
                return false;
            }

            if (!ServiceLocator.TryGet(out UnitPlacementValidator placement))
            {
                failReason = "Unit placement validator not found.";
                return false;
            }

            if (!placement.CanPlace(player, tile, out failReason))
                return false;

            newUnit = UnitPool.Instance.Get(player.Team);
            if (newUnit == null)
            {
                failReason = "Unit pool returned null.";
                return false;
            }

            UnitModel model = new(cardModel, cardModel.UnitData, player.Team);
            newUnit.Initialize(model, tile);
            tile.PlaceUnit(newUnit);

            if (ServiceLocator.TryGet(out UnitManager unitManager))
                unitManager.RegisterUnit(newUnit);

            return true;
        }
    }
}