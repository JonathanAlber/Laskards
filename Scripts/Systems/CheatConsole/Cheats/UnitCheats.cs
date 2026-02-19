using Gameplay.Units;
using Systems.Services;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Cheat commands for manipulating unit states.
    /// </summary>
    public sealed class UnitCheats : MonoBehaviour
    {
        [CheatCommand("kill_all_boss_units", Description = "Kills all boss units currently in play.",
            Usage = "kill_all_boss_units")]
        private void KillAllBossUnits()
        {
            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return;

            // Reverse iterate to avoid issues with collection modification during iteration
            for (int i = unitManager.BossUnits.Count - 1; i >= 0; i--)
            {
                UnitController bossUnit = unitManager.BossUnits[i];
                bossUnit.Die();
            }
        }

        [CheatCommand("kill_all_player_units", Description = "Kills all player units currently in play.",
            Usage = "kill_all_player_units")]
        private void KillAllPlayerUnits()
        {
            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return;

            // Reverse iterate to avoid issues with collection modification during iteration
            for (int i = unitManager.PlayerUnits.Count - 1; i >= 0; i--)
            {
                UnitController playerUnit = unitManager.PlayerUnits[i];
                playerUnit.Die();
            }
        }
    }
}