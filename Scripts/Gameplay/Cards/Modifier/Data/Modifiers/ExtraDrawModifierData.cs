using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing an extra draw modifier in the game.
    /// It allows a player to draw additional cards each turn as long as the modifier is active.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_ExtraDrawModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Extra Draw Modifier")]
    public class ExtraDrawModifierData : ModifierData
    {
        [field: Header("Extra Draw Modifier Settings")]
        [field: Tooltip("Number of extra cards the player can draw.")]
        [field: SerializeField] public int ExtraCardsToDraw { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Resource;

        public override ModifierRuntimeState CreateRuntimeState() => new ExtraDrawModifierRuntimeState(this);
    }
}