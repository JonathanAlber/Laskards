using Gameplay.Cards.Data.States;
using UnityEngine;

namespace Gameplay.Units.Data
{
    /// <summary>
    /// Data container for a specific unit types properties and visuals.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitData", menuName = "ScriptableObjects/Units/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [field: Header("Unit Card Data")]
        [field: Tooltip("The state and properties specific to this unit card.")]
        [field: SerializeField] public UnitCardState Unit { get; private set; }

        [field: SerializeField] public EUnitType UnitType { get; private set; }

        [field: Header("Visuals")]
        [field: SerializeField] public Sprite UnitSprite { get; private set; }
    }
}