using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing an attack modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_AttackBonusModifier",
        menuName = "ScriptableObjects/Cards/Modifier/AttackBonusModifierData")]
    public class AttackBonusModifierData : ModifierData
    {
        [Header("Attack Modifier Settings")]
        [Tooltip("The amount to increase the unit's attack by.")]
        [SerializeField] private int attackIncrease;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new AttackModifierRuntimeState(this, attackIncrease);
        }
    }
}