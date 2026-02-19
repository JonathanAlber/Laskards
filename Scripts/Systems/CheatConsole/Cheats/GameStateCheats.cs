using Systems.PriorityTrackers;
using Systems.Services;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Contains cheat commands related to game state manipulation.
    /// </summary>
    public class GameStateCheats : MonoBehaviour
    {
        private const uint TimescalePriority = 100;

        [CheatCommand("set_timescale", Description = "Sets the game's timescale to the specified value.",
            Usage = "set_timescale <scale>")]
        private string SetTimescale(float scale)
        {
            ServiceLocator.Get<TimeScaleManager>()?.TimeScaleTracker.Add(scale, TimescalePriority, this);
            return $"Set time scale to {scale} with priority {TimescalePriority}.";
        }
    }
}