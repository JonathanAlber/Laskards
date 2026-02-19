using Gameplay.Cards.Modifier.Data;
using UnityEngine;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// ScriptableObject definition for an <see cref="ActionCardController"/> in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "ACD_ActionCardDefinition", menuName = "ScriptableObjects/Cards/Card Definitions/Action Card")]
    public class ActionCardDefinition : CardDefinition
    {
        [field: Header("Action Card Data")]

        [field: Tooltip("The modifier applied when this card is played.")]
        [field: SerializeField] public ModifierData Modifier { get; private set; }

        [field: TextArea(1, 2)]
        [field: Tooltip("Description text shown in tooltips or UI.")]
        [field: SerializeField] public string Description { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => Modifier?.TeamAffiliation ?? ECardTeamAffiliation.Both;
    }
}