using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that automatically tries to draw Type A cards when a special unit dies.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_DrawTypeAOnSpecialUnitDeathModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Draw Type A On Special Unit Death Modifier")]
    public class DrawTypeAOnSpecialUnitDeathModifierData : ModifierData
    {
        [Header("Draw Type A On Special Unit Death Modifier Settings")]
        [Tooltip("The amount of Type A cards to draw when a special unit dies.")]
        [SerializeField] private int amountToDraw = 1;

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Player;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new DrawTypeAOnSpecialUnitDeathModifierRuntimeState(this, amountToDraw);
        }
    }
}