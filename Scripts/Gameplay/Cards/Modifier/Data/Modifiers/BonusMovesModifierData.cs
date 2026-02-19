using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a bonus moves modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_BonusMovesModifier",
        menuName = "ScriptableObjects/Cards/Modifier/BonusMovesModifierData")]
    public class BonusMovesModifierData : ModifierData
    {
        [Header("Bonus Moves Modifier Settings")]
        [Tooltip("The number of bonus moves to grant to the unit.")]
        [SerializeField] private int bonusMoves = 1;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new BonusMovesModifierRuntimeState(this, bonusMoves);
        }
    }
}