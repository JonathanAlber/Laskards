using System;
using UnityEngine;

namespace Gameplay.Cards.Data.States
{
    /// <summary>
    /// Common immutable properties shared by all card types.
    /// These properties are set via the inspector and define basic card information.
    /// They can be modified at runtime if needed, by using the provided setter methods.
    /// </summary>
    [Serializable]
    public class CardCommonState
    {
        [field: Tooltip("The display name shown in-game.")]
        [field: SerializeField] public string DisplayName { get; private set; }

        [field: Tooltip("The image used to represent the card.")]
        [field: SerializeField] public Sprite CardImage { get; private set; }

        [field: Tooltip("Base energy cost to play this card.")]
        [field: SerializeField] public int EnergyCost { get; private set; }

        /// <summary>
        /// Sets a new energy cost for the card at runtime.
        /// </summary>
        public void SetEnergyCost(int newCost) => EnergyCost = newCost;

        /// <summary>
        /// Creates a deep runtime copy so ScriptableObject data is never modified.
        /// </summary>
        /// <returns>A new instance with the same property values.</returns>
        public CardCommonState Clone()
        {
            return new CardCommonState
            {
                DisplayName = DisplayName,
                CardImage = CardImage,
                EnergyCost = EnergyCost
            };
        }

        private CardCommonState() { }
    }
}