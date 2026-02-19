using UnityEngine;
using Utility.Logging;

namespace Gameplay.CardExecution.Data
{
    /// <summary>
    /// Configures the allowed deployment zones for player or boss units on the board.
    /// </summary>
    [CreateAssetMenu(fileName = "DeploymentZones", menuName = "ScriptableObjects/Board/Deployment Zones")]
    public class DeploymentZonesConfig : ScriptableObject
    {
        [field: Min(0)]
        [field: SerializeField]
        [field: Tooltip("How many rows from the bottom the player may place within. 0 = no restriction.")]
        public int PlayerRowsFromBottom { get; private set; } = 1;

        [field: Min(0)]
        [field: SerializeField]
        [field: Tooltip("How many rows from the top the boss may place within. 0 = no restriction.")]
        public int BossRowsFromTop { get; private set; } = 1;

        /// <summary>
        /// Determines if a given row index is allowed for the specified team.
        /// </summary>
        public bool IsRowAllowed(ETeam team, int rowIndex, int totalRows)
        {
            switch (team)
            {
                case ETeam.Player:
                {
                    if (PlayerRowsFromBottom <= 0)
                        return true;

                    int maxAllowed = PlayerRowsFromBottom - 1;
                    return rowIndex <= maxAllowed;
                }
                case ETeam.Boss:
                {
                    if (BossRowsFromTop <= 0)
                        return true;

                    int minAllowed = totalRows - BossRowsFromTop;
                    return rowIndex >= minAllowed;
                }
                default:
                {
                    CustomLogger.LogError($"Unknown team type: {team}", this);
                    return false;
                }
            }
        }
    }
}