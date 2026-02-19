using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that instantly draws
    /// a set amount of cards fron a defined deck automatically for the player.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_InstantDrawModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Instant Draw Modifier")]
    public class InstantDrawModifierData : ModifierData
    {
        [field: Header("Draw Cards Modifier Settings")]

        [field: Tooltip("Number of cards the player draws manually.")]
        [field: SerializeField] public int CardsToDraw { get; private set; }

        [field: Tooltip("The deck from which the cards will be drawn.")]
        [field: SerializeField] public EDeck Deck { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Resource;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new InstantDrawModifierRuntimeState(this);
        }
    }
}