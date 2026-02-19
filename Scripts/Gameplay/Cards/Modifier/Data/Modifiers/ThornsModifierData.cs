using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a thorns modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_ThornsModifier", menuName = "ScriptableObjects/Cards/Modifier/ThornsModifierData")]
    public class ThornsModifierData : ModifierData
    {
        [Header("Thorns Modifier Settings")]
        [Tooltip("The percentage of damage to reflect back to the attacker." +
                 " The final value will be rounded to the nearest integer.")]
        [SerializeField] private float damageReflectionPercentage;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new ThornsModifierRuntimeState(this, damageReflectionPercentage);
        }
    }
}