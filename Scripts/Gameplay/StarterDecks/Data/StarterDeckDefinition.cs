using System.Collections.Generic;
using Gameplay.Cards.Data;
using UnityEngine;
using Utility.Identification;

namespace Gameplay.StarterDecks.Data
{
    /// <summary>
    /// ScriptableObject representing a starter deck of cards for players to choose from at the start of the game.
    /// </summary>
    [CreateAssetMenu(fileName = "StarterDeck", menuName = "ScriptableObjects/Decks/Starter Deck")]
    public class StarterDeckDefinition : ScriptableObject, IUniquelyIdentifiable
    {
        [field: Space]
        [field: SerializeField, Tooltip("Deck name shown in UI.")]
        public string DisplayName { get; private set; } = "Starter Deck";

        [field: SerializeField, TextArea(1, 3), Tooltip("Description of the starter deck shown in UI.")]
        public string Description { get; private set; }

        [field: SerializeField, Tooltip("Image of the bird associated with this starter deck.")]
        public Sprite BirdImage { get; private set; }

        [field: SerializeField, Tooltip("Cards included when the player chooses this starter deck.")]
        public List<CardDefinition> Cards { get; private set; } = new();

        [field: SerializeField, HideInInspector] public string UniqueId { get; private set; }
    }
}