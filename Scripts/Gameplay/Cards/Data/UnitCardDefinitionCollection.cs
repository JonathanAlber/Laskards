using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// A ScriptableObject that holds a collection of all available unit card definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitCardDefinitionCollection",
        menuName = "ScriptableObjects/Cards/Unit Card Collection")]
    public class UnitCardDefinitionCollection : ScriptableObject
    {
        [field: Tooltip("All available unit cards for the game, e.g. for the Boss to use.")]
        [field: SerializeField] public List<UnitCardDefinition> Cards { get; private set; } = new();
    }
}