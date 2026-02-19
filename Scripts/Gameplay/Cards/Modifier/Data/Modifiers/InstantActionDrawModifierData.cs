using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;

namespace Gameplay.Cards.Modifier.Data.Modifiers
{
    /// <summary>
    /// ScriptableObject representing a modifier that instantly draws a set amount of action cards
    /// for the player from a specified action category.
    /// </summary>
    [CreateAssetMenu(fileName = "MD_InstantActionDrawModifier",
        menuName = "ScriptableObjects/Cards/Modifier/Instant Action Draw Modifier")]
    public class InstantActionDrawModifierData : ModifierData
    {
        [field: Header("Draw Cards Modifier Settings")]

        [field: Tooltip("Number of cards the player draws manually.")]
        [field: SerializeField] public int CardsToDraw { get; private set; }

        [field: Tooltip("If true, the drawn cards will be free to play.")]
        [field: SerializeField] public bool MakeCardsFree { get; private set; }

        [field: Tooltip("The category of action cards to draw.")]
        [field: SerializeField] public EActionCategory ActionCategoryToDraw { get; private set; }

        public override ECardTeamAffiliation TeamAffiliation => ECardTeamAffiliation.Player;

        public override EActionCategory ActionCategory => EActionCategory.Resource;

        public override ModifierRuntimeState CreateRuntimeState()
        {
            return new InstantActionDrawModifierRuntimeState(this);
        }
    }
}