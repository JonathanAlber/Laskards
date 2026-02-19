using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a health modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_HealthBonusModifier",
        menuName = "ScriptableObjects/Cards/Modifier/HealthBonusModifierData")]
    public class HealthBonusModifierData : ModifierData
    {
        [Header("Health Modifier Settings")]
        [Tooltip("The amount to increase the unit's health by.")]
        [SerializeField] private int gainedHealth;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new HealthBonusModifierRuntimeState(this, gainedHealth);
        }
    }
}