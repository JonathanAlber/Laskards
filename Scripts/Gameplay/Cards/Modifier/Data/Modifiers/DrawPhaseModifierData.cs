using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that enables the player to draw new cards manually.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_DrawPhaseModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Draw Phase Modifier")]
    public class DrawPhaseModifierData : ModifierData
    {
        [field: Header("Draw Cards Modifier Settings")]
        [field: Tooltip("Number of cards the player draws manually.")]
        [field: SerializeField] public int CardsToDraw { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Resource;

        public override ModifierRuntimeState CreateRuntimeState() => new DrawPhaseModifierRuntimeState(this);
    }
}