using Gameplay.Player;
using Systems.Services;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Cheat commands for manipulating the player's state.
    /// </summary>
    public class PlayerStateCheats : MonoBehaviour
    {
        private const float RoundDefaultEnergy = 50f;
        private const float RoundDefaultDraws = 20f;

        [CheatCommand("player_cheat_default_state", Description = "Sets the player's energy and draws to the" +
            " defined defaults.", Usage = "player_state_default")]
        private void SetPlayerDefault()
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            PlayerSnapshot next = player.Snapshot.WithEnergySpent(player.Snapshot.CurrentEnergy); // reset to 0
            next = next
                .WithEnergyGained(Mathf.Max(0, (int)RoundDefaultEnergy))
                .WithCardsToDrawRemaining((int)RoundDefaultDraws);

            player.ApplySnapshot(next);
        }

        [CheatCommand("lose", Description = "Kills the player instantly.", Usage = "lose")]
        private void KillPlayer()
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            player.TakeDamage(player.Snapshot.CurrentHp);
        }

        [CheatCommand("set_energy", Description = "Sets the player's current energy.", Usage = "set_energy <amount>")]
        private void SetEnergy(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            PlayerSnapshot next = player.Snapshot.WithEnergySpent(player.Snapshot.CurrentEnergy); // reset to 0
            next = next.WithEnergyGained(Mathf.Max(0, amount));
            player.ApplySnapshot(next);
        }

        [CheatCommand("add_energy", Description = "Adds (or subtracts) energy to the player.",
            Usage = "add_energy <delta>")]
        private void AddEnergy(int delta)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            PlayerSnapshot next = player.Snapshot.WithEnergyGained(delta);
            player.ApplySnapshot(next);
        }

        [CheatCommand("set_energy_gain_per_round", Description = "Sets how much energy you gain each round (base).",
            Usage = "set_energy_gain_per_round <amount>")]
        private void SetEnergyGainPerRound(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newValue = Mathf.Max(0, amount);
            PlayerSnapshot next = player.Snapshot.WithEnergyGainPerRound(newValue);
            player.ApplySnapshot(next);
        }

        [CheatCommand("add_energy_gain_per_round", Description = "Adds (or subtracts) energy gain per round (base).",
            Usage = "add_energy_gain_per_round <delta>")]
        private void AddEnergyGainPerRound(int delta)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newValue = Mathf.Max(0, player.Snapshot.EnergyGainPerRound + delta);
            PlayerSnapshot next = player.Snapshot.WithEnergyGainPerRound(newValue);
            player.ApplySnapshot(next);
        }

        [CheatCommand("set_draws", Description = "Sets the number of cards remaining to draw this round.",
            Usage = "set_draws <amount>")]
        private void SetDraws(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newBase = Mathf.Max(0, amount);
            PlayerSnapshot next = player.Snapshot.WithCardsToDrawRemaining(newBase);

            player.ApplySnapshot(next);
        }

        [CheatCommand("add_draws", Description = "Adds (or subtracts) to the number of cards remaining to " +
                                                 "draw this round.", Usage = "add_draws <delta>")]
        private void AddDraws(int delta)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newRemaining = Mathf.Max(0, player.Snapshot.CardsToDraw + delta);

            PlayerSnapshot next = player.Snapshot.WithCardsToDrawRemaining(newRemaining);

            player.ApplySnapshot(next);
        }

        [CheatCommand("set_draws_per_round", Description = "Sets the number of cards drawn each round (base).",
            Usage = "set_draws_per_round <amount>")]
        private void SetDrawAmountPerRound(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newBase = Mathf.Max(0, amount);

            PlayerSnapshot next = player.Snapshot.WithCardsToDrawPerRound(newBase);

            player.ApplySnapshot(next);
        }

        [CheatCommand("add_draws_per_round", Description = "Adds (or subtracts) to the number of cards drawn each" +
                                                           " round (base).", Usage = "add_draws_per_round <delta>")]
        private void AddDrawAmountPerRound(int delta)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int newBase = Mathf.Max(0, player.Snapshot.CardsToDrawPerRound + delta);

            PlayerSnapshot next = player.Snapshot.WithCardsToDrawPerRound(newBase);

            player.ApplySnapshot(next);
        }

        [CheatCommand("player_set_health", Description = "Sets the player's health to the given amount.",
            Usage = "player_set_health <amount>")]
        private void SetHealth(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            int clamped = Mathf.Clamp(amount, 0, player.Snapshot.MaxHealth);
            PlayerSnapshot next = new(
                player.Snapshot.MaxHealth,
                clamped,
                player.Snapshot.CurrentEnergy,
                player.Snapshot.EnergyGainPerRound,
                player.Snapshot.CardsToDrawPerRound,
                player.Snapshot.CardsToDraw
            );

            player.ApplySnapshot(next);
        }

        [CheatCommand("player_heal", Description = "Heals the player by the given amount.",
            Usage = "player_heal <amount>")]
        private void Heal(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            player.Heal(amount);
        }

        [CheatCommand("player_damage", Description = "Damages the player by the given amount.",
            Usage = "player_damage <amount>")]
        private void Damage(int amount)
        {
            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            player.TakeDamage(amount);
        }
    }
}