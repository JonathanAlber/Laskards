using Gameplay.Units.Data;
using UnityEngine;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// ScriptableObject definition for a <see cref="UnitCardController"/> in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "UCD_UnitCardDefinition", menuName = "ScriptableObjects/Cards/Card Definitions/Unit Card")]
    public class UnitCardDefinition : CardDefinition
    {
        [field: Tooltip("The associated unit data for the unit this card spawns.")]
        [field: SerializeField] public UnitData UnitData { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;
    }
}