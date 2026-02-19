using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that automatically tries to draw Type A cards when a unit dies.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_DrawTypeAOnUnitDeathModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Draw Type A On Unit Death Modifier")]
    public class DrawTypeAOnUnitDeathModifierData : ModifierData
    {
        [Header("Draw Type A On Unit Death Modifier Settings")]
        [Tooltip("The amount of Type A cards to draw when a unit dies.")]
        [SerializeField] private int amountToDraw = 1;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;
        public override EActionCategory ActionCategory => EActionCategory.Player;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new DrawTypeAOnUnitDeathModifierRuntimeState(this, amountToDraw);
        }
    }
}