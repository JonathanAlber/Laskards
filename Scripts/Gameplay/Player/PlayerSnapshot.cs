using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Immutable value object representing the player's runtime stats.
    /// </summary>
    public readonly struct PlayerSnapshot
    {
        /// <summary>
        /// The maximum health of the player.
        /// </summary>
        public int MaxHealth { get; }

        /// <summary>
        /// The current health of the player.
        /// </summary>
        public int CurrentHp { get; }

        /// <summary>
        /// Current energy of the player.
        /// </summary>
        public int CurrentEnergy { get; }

        /// <summary>
        /// Base energy gained each round (before effects).
        /// </summary>
        public int EnergyGainPerRound { get; }

        /// <summary>
        /// Base number of cards to draw each round (before effects).
        /// </summary>
        public int CardsToDrawPerRound { get; }

        /// <summary>
        /// Cards remaining to draw this round.
        /// </summary>
        public int CardsToDraw { get; }

        /// <summary>
        /// If the player is dead (0 or less HP).
        /// </summary>
        public bool IsDead => CurrentHp <= 0;

        public PlayerSnapshot(int maxHealth, int currentHp, int currentEnergy, int energyGainPerRound,
            int cardsToDrawPerRound, int cardsToDrawRemaining)
        {
            MaxHealth = Mathf.Max(0, maxHealth);
            CurrentEnergy = Mathf.Max(0, currentEnergy);
            CurrentHp = Mathf.Clamp(currentHp, 0, MaxHealth);

            EnergyGainPerRound = Mathf.Max(0, energyGainPerRound);
            CardsToDrawPerRound = Mathf.Max(0, cardsToDrawPerRound);
            CardsToDraw = Mathf.Max(0, cardsToDrawRemaining);
        }

        /// <summary>
        /// Create the initial snapshot from player state data.
        /// </summary>
        /// <param name="data">The data configuration.</param>
        /// <returns>The created snapshot.</returns>
        public static PlayerSnapshot FromData(PlayerStateData data) =>
            new(data.MaxHealth, data.MaxHealth, 0, data.BaseEnergyPerRound,
                data.BaseCardsToDrawPerTurn, 0);

        /// <summary>
        /// Return a new snapshot with damage applied.
        /// </summary>
        public PlayerSnapshot WithDamage(int amount)
        {
            int dmg = Mathf.Max(0, amount);
            return new PlayerSnapshot(MaxHealth, Mathf.Max(0, CurrentHp - dmg), CurrentEnergy,
                EnergyGainPerRound, CardsToDrawPerRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with healing applied.
        /// </summary>
        public PlayerSnapshot WithHeal(int amount)
        {
            int heal = Mathf.Max(0, amount);
            return new PlayerSnapshot(MaxHealth, Mathf.Min(MaxHealth, CurrentHp + heal), CurrentEnergy,
                EnergyGainPerRound, CardsToDrawPerRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with energy gained.
        /// </summary>
        public PlayerSnapshot WithEnergyGained(int amount)
        {
            int gain = Mathf.Max(0, amount);
            return new PlayerSnapshot(MaxHealth, CurrentHp, CurrentEnergy + gain,
                EnergyGainPerRound, CardsToDrawPerRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with energy spent.
        /// </summary>
        public PlayerSnapshot WithEnergySpent(int cost)
        {
            int c = Mathf.Max(0, cost);
            return new PlayerSnapshot(MaxHealth, CurrentHp, Mathf.Max(0, CurrentEnergy - c),
                EnergyGainPerRound, CardsToDrawPerRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with changed base energy gain per round.
        /// </summary>
        public PlayerSnapshot WithEnergyGainPerRound(int energyGain)
        {
            int gain = Mathf.Max(0, energyGain);
            return new PlayerSnapshot(MaxHealth, CurrentHp, CurrentEnergy,
                gain, CardsToDrawPerRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with changed base cards to draw per round.
        /// </summary>
        public PlayerSnapshot WithCardsToDrawPerRound(int cardsToDrawPerRound)
        {
            int perRound = Mathf.Max(0, cardsToDrawPerRound);
            return new PlayerSnapshot(MaxHealth, CurrentHp, CurrentEnergy,
                EnergyGainPerRound, perRound, CardsToDraw);
        }

        /// <summary>
        /// Return a new snapshot with changed remaining cards to draw this round.
        /// </summary>
        public PlayerSnapshot WithCardsToDrawRemaining(int remaining)
        {
            int rem = Mathf.Max(0, remaining);
            return new PlayerSnapshot(MaxHealth, CurrentHp, CurrentEnergy,
                EnergyGainPerRound, CardsToDrawPerRound, rem);
        }

        /// <summary>
        /// Check if the player can draw the specified number of cards.
        /// </summary>
        /// <param name="cardsToDraw">The number of cards to draw.</param>
        /// <returns><c>true</c> if the player can draw the cards; otherwise, <c>false</c>.</returns>
        public bool CanDrawCards(int cardsToDraw) => CardsToDraw >= Mathf.Max(0, cardsToDraw);

        /// <summary>
        /// Check if the player can spend the specified energy cost.
        /// </summary>
        /// <param name="cost">The energy cost to check.</param>
        /// <returns><c>true</c> if the player can spend the energy; otherwise, <c>false</c>.</returns>
        public bool CanSpend(int cost) => CurrentEnergy >= Mathf.Max(0, cost);
    }
}