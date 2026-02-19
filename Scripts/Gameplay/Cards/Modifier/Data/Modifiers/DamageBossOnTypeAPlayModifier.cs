using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that damages the boss when a Type A card is played.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_DamageBossOnTypeAPlayModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Damage Boss On Type A Play Modifier")]
    public class DamageBossOnTypeAPlayModifier : ModifierData
    {
        [Header("Damage Boss On Type A Play Modifier Settings")]
        [Tooltip("The amount of damage dealt to the boss when a Type A card is played.")]
        [SerializeField, Min(0)] private int damageAmount = 1;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Player;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new DamageBossOnTypeAPlayRuntimeState(this, damageAmount);
        }
    }
}