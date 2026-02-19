using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a clear tile modifier in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_ClearTileModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Clear Tile Modifier")]
    public class ClearTileModifierData : ModifierData
    {
        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Both;

        public override EActionCategory ActionCategory => EActionCategory.Tile;

        public override ModifierRuntimeState CreateRuntimeState() => new ClearTileModifierRuntimeState(this);
    }
}