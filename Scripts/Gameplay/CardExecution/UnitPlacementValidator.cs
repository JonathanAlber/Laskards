using Gameplay.Board;
using Gameplay.CardExecution.Data;
using Gameplay.Units;
using Systems.Services;
using UnityEngine;

namespace Gameplay.CardExecution
{
    /// <summary>
    /// Validates whether a <see cref="UnitController"/> can be placed
    /// on a specific <see cref="Tile"/> according to game rules.
    /// </summary>
    public sealed class UnitPlacementValidator : GameServiceBehaviour
    {
        [field: Tooltip("Deployment zone configuration for unit placement rules.")]
        [field: SerializeField] public DeploymentZonesConfig Deployment { get; private set; }

        /// <summary>
        /// Checks if a unit can be placed on the specified tile by the given player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tile"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool CanPlace(ICardPlayer player, Tile tile, out string reason)
        {
            reason = null;

            if (!ServiceLocator.TryGet(out GameBoard board))
            {
                reason = "Game board not found.";
                return false;
            }

            if (tile == null)
            {
                reason = "No tile.";
                return false;
            }

            if (tile.IsOccupied())
            {
                reason = "Tile is occupied.";
                return false;
            }

            ETeam team = player.Team;
            int tileRow = tile.Row;
            int totalRows = board.BoardConfiguration.Rows;
            if (Deployment.IsRowAllowed(team, tileRow, totalRows))
                return true;

            int row = team == ETeam.Player ? Deployment.PlayerRowsFromBottom : Deployment.BossRowsFromTop;
            int effectiveRow = team == ETeam.Player ? tileRow + 1 : totalRows - tileRow;
            string plural = row > 1 ? "s" : "";

            reason = team == ETeam.Player
                ? $"Place only in your front {row} row{plural} instead of {effectiveRow}."
                : $"Boss can place only in their back {row} row{plural}, but tried to place in {effectiveRow}.";

            return false;
        }
    }
}