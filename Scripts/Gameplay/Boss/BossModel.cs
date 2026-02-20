using System;
using Gameplay.Boss.Data;
using Utility.Logging;

namespace Gameplay.Boss
{
    /// <summary>
    /// Holds all runtime state and immutable data for the boss.
    /// </summary>
    public class BossModel
    {
        /// <summary>
        /// Event invoked when the boss takes damage.
        /// The int parameter is the amount of damage taken.
        /// </summary>
        public static event Action<int> OnDamageTaken;

        /// <summary>
        /// Event invoked when the boss becomes aggressive.
        /// </summary>
        public static event Action OnAggressive;

        /// <summary>
        /// Event invoked when the boss's health has changed.
        /// The int parameter is the current health value.
        /// </summary>
        public event Action OnHealthChanged;

        /// <summary>
        /// Event invoked when the boss is defeated.
        /// </summary>
        public event Action OnDeath;

        /// <summary>
        /// Current boss health.
        /// </summary>
        public int CurrentHp { get; private set; }

        /// <summary>
        /// True if the boss is currently aggressive.
        /// </summary>
        public bool IsAggressive { get; private set; }

        /// <summary>
        /// Health percentage threshold below which the boss becomes aggressive.
        /// </summary>
        public readonly float AggressiveThreshold;

        /// <summary>
        /// Maximum boss health.
        /// </summary>
        public readonly int MaxHealth;

        /// <summary>
        /// Creates a new model instance from definition data.
        /// </summary>
        /// <param name="data">The boss definition.</param>
        public BossModel(BossData data)
        {
            if (data == null)
            {
                CustomLogger.LogError($"{nameof(BossData)} is null during model initialization.", null);
                return;
            }

            MaxHealth = data.Health;
            AggressiveThreshold = data.AggressiveHealthThreshold;

            CurrentHp = MaxHealth;
        }

        /// <summary>
        /// Applies damage and returns true if the boss has fallen.
        /// </summary>
        /// <param name="amount">Damage to apply.</param>
        /// <returns><c>true</c> if the boss is defeated; otherwise, <c>false</c>.</returns>
        public void ApplyDamage(int amount)
        {
            if (amount < 0)
                amount = 0;

            // If not aggressive yet clamp health to aggressive threshold
            if (!IsAggressive)
            {
                int aggressiveHpThreshold = (int)(MaxHealth * AggressiveThreshold);
                int newHp = CurrentHp - amount;
                if (newHp < aggressiveHpThreshold)
                    newHp = aggressiveHpThreshold;

                SetHealth(newHp);
            }
            else
            {
                SetHealth(CurrentHp - amount);
            }

            OnDamageTaken?.Invoke(amount);
        }

        /// <summary>
        /// Sets the boss's current health to the specified amount.
        /// </summary>
        /// <param name="amount">New health amount.</param>
        public void SetHealth(int amount)
        {
            if (amount < 0)
                amount = 0;

            if (amount > MaxHealth)
                amount = MaxHealth;

            if (amount == CurrentHp)
                return;

            CurrentHp = amount;
            RaiseStateEvents();
        }

        private void RaiseStateEvents()
        {
            float normalized = MaxHealth > 0
                ? CurrentHp / (float)MaxHealth
                : 0f;

            OnHealthChanged?.Invoke();

            bool shouldBecomeAggressive = !IsAggressive && normalized <= AggressiveThreshold && MaxHealth > 0;
            if (shouldBecomeAggressive)
            {
                IsAggressive = true;
                OnAggressive?.Invoke();
            }

            if (CurrentHp <= 0)
                OnDeath?.Invoke();
        }
    }
}