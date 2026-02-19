using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing an extra energy modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_ExtraEnergyModifier", menuName = "ScriptableObjects/Cards/Modifier/Extra Energy Modifier")]
    public class ExtraEnergyModifierData : ModifierData
    {
        [field: Header("Extra Energy Modifier Settings")]
        [field: Tooltip("Amount of extra energy the player can gain.")]
        [field: SerializeField] public int ExtraEnergyToGain { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;
        public override EActionCategory ActionCategory => EActionCategory.Player;

        public override ModifierRuntimeState CreateRuntimeState() => new ExtraEnergyModifierRuntimeState(this);
    }
}