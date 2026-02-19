using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// A ScriptableObject that holds a collection of <see cref="CardDefinition"/>
    /// assets that can be used by the player.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCardDefinitionCollection",
        menuName = "ScriptableObjects/Cards/Player Card Collection")]
    public class PlayerCardDefinitionCollection : ScriptableObject
    {
        [field: Tooltip("All cards that the player can use in the game, e.g. the shop.")]
        [field: SerializeField] public List<CardDefinition> Cards { get; private set; } = new();
    }
}