using System;
using UnityEngine;

namespace Gameplay.Cards.Data.States
{
    /// <summary>
    /// Authoring-time definition of a unit card's unit stats.
    /// </summary>
    [Serializable]
    public class UnitCardState
    {
        [field: Tooltip("Base damage value of this unit.")]
        [field: SerializeField] public int Damage { get; private set; }

        [field: Tooltip("How many turns this unit remains on the field before expiring. -1 for infinite.")]
        [field: SerializeField] public int Lifetime { get; private set; }

        [field: Tooltip("Base health of this unit.")]
        [field: SerializeField] public int BaseHealth { get; private set; }

        [field: Tooltip("How much this unit is worth when defeated.")]
        [field: SerializeField] public int Worth { get; private set; }

        /// <summary>
        /// Sets the damage value of this unit.
        /// </summary>
        /// <param name="newDamage"></param>
        public void SetDamage(int newDamage) => Damage = newDamage;

        /// <summary>
        /// Sets the base health of this unit.
        /// </summary>
        /// <param name="newHealth"></param>
        public void SetBaseHealth(int newHealth) => BaseHealth = newHealth;

        /// <summary>
        /// Creates a deep runtime copy so ScriptableObject data is never modified.
        /// </summary>
        /// <returns>A new instance with the same property values.</returns>
        public UnitCardState Clone()
        {
            return new UnitCardState
            {
                Damage = Damage,
                Lifetime = Lifetime,
                BaseHealth = BaseHealth,
                Worth = Worth
            };
        }

        private UnitCardState() { }
    }
}