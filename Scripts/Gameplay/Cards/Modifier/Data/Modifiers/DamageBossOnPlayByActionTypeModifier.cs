using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that damages the boss when a card of a specific action type is played.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_DamageBossOnPlayByTypeModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Damage Boss On Play By Type Modifier")]
    public class DamageBossOnPlayByActionTypeModifier : ModifierData
    {
        [Header("Damage Boss On Draw By Type Modifier Settings")]
        [Tooltip("The action type that triggers the damage to the boss when a card of this type is played.")]
        [SerializeField] private EActionCategory actionType;

        [Tooltip("The amount of damage dealt to the boss when a card of the specified action type is played.")]
        [SerializeField, Min(0)] private int damageAmount = 1;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Player;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new DamageBossOnPlayByTypeRuntimeState(this, actionType, damageAmount);
        }
    }
}