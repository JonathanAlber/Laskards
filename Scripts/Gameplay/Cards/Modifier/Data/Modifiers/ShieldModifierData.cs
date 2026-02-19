using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a shield modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_ShieldModifier", menuName = "ScriptableObjects/Cards/Modifier/Shield Modifier")]
    public class ShieldModifierData : ModifierData
    {
        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;
        public override EActionCategory ActionCategory => EActionCategory.Buff;

        public override ModifierRuntimeState CreateRuntimeState() => new ShieldModifierRuntimeState(this);
    }
}