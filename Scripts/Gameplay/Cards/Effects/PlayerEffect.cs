using System;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that can be applied to a player in the game.
    /// </summary>
    public class PlayerEffect : Effect<PlayerController>
    {
        /// <summary>
        /// Event invoked when the boss is damaged by this effect.
        /// </summary>
        public event Action OnDamagedBoss;

        public PlayerEffect(PlayerController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData)
            : base(target, creatorTeam, durationType, duration, effectData) {}

        /// <summary>
        /// The number of additional cards the player can draw each turn.
        /// </summary>
        public int AdditionalActionCardDraws { get; protected set; }

        /// <summary>
        /// The amount of additional energy the player gains each turn.
        /// </summary>
        public int AdditionalEnergyGain { get; protected set; }

        /// <summary>
        /// Raises the <see cref="OnDamagedBoss"/> event.
        /// </summary>
        protected void RaiseDamagedBoss() => OnDamagedBoss?.Invoke();
    }
}