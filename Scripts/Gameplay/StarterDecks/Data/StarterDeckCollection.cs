using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StarterDecks.Data
{
    /// <summary>
    /// ScriptableObject representing a collection of starter decks available in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "StarterDeckCollection", menuName = "ScriptableObjects/Decks/Starter Deck Collection")]
    public class StarterDeckCollection : ScriptableObject
    {
        [field: SerializeField, Tooltip("Starter decks available for use in-game.")]
        public List<StarterDeckDefinition> Decks { get; private set; } = new();
    }
}