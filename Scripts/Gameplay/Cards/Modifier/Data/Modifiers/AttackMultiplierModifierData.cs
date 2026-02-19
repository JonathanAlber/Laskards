using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing an attack multiplier modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_AttackMultiplierModifier",
        menuName = "ScriptableObjects/Cards/Modifier/AttackMultiplierModifierData")]
    public class AttackMultiplierModifierData : ModifierData
    {
        [Header("Attack Multiplier Modifier Settings")]
        [Tooltip("The multiplier to apply to the unit's attack. Non integer values will be rounded.")]
        [SerializeField] private float attackMultiplier = 2;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new AttackMultiplierModifierRuntimeState(this, attackMultiplier);
        }
    }
}