using Gameplay.Cards.Data.States;
using UnityEngine;
using Utility.Identification;

namespace Gameplay.Cards.Data
{
    /// <summary>
    /// Immutable authoring-time definition of a card.
    /// Concrete card types (Unit / Action / etc.) derive from this.
    /// </summary>
    public abstract class CardDefinition : ScriptableObject, IUniquelyIdentifiable
    {
        [field: Header("Common Card Info")]
        [field: Tooltip("Common properties shared by all card types.")]
        [field: SerializeField] public CardCommonState Common { get; private set; }

        [field: SerializeField, HideInInspector] public string UniqueId { get; private set; }

        /// <summary>
        /// Which card players can use this card and which card collections can hold it.
        /// </summary>
        public abstract ECardTeamAffiliation TeamAffiliation { get; }
    }
}