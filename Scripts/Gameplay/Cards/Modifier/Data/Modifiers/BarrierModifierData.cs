using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a barrier modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_BarrierModifier", menuName = "ScriptableObjects/Cards/Modifier/Barrier Modifier")]
    public class BarrierModifierData : ModifierData
    {
        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;
        public override EActionCategory ActionCategory => EActionCategory.Tile;

        public override ModifierRuntimeState CreateRuntimeState() => new BarrierModifierRuntimeState(this);

    }
}