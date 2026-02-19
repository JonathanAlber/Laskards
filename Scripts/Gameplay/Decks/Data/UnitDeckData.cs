using Gameplay.Cards.Data;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Decks.Data
{
    /// <summary>
    /// Defines which base unit card and how many of them form the unit deck.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDeckDefinition", menuName = "ScriptableObjects/Decks/Unit Deck Definition")]
    public class UnitDeckData : ScriptableObject
    {
        [field: SerializeField, Tooltip("The base unit card this deck consists of.")]
        public CardDefinition BaseUnitCard { get; private set; }

        [field: SerializeField, Min(1), Tooltip("How many unit cards are included in this deck.")]
        public int MaxUnits { get; private set; } = 8;

#if UNITY_EDITOR
        [ContextMenu("Validate")]
        private void ValidateDeck()
        {
            if (BaseUnitCard == null)
                CustomLogger.LogWarning($"{name} has no base unit card assigned.", this);
        }
#endif
    }
}