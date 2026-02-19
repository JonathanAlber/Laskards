using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Cards.Modifier.Data.Runtime;
using UnityEngine;
using Utility.Identification;

namespace Gameplay.Cards.Modifier.Data
{
    /// <summary>
    /// Base class for all modifier data types in the game.
    /// </summary>
    public abstract class ModifierData : ScriptableObject, IUniquelyIdentifiable
    {
        [field: Header("Basic Information")]
        [field: Tooltip("The effect data for UI representation.")]
        [field: SerializeField] public EffectData EffectData { get; private set; }

        [field: Header("Stats")]
        [field: Tooltip("The duration type of the modifier.")]
        [field: SerializeField] public EDurationType DurationType { get; private set; }

        [field: Tooltip("The duration of the modifier in turns. Relevant only if DurationType is Temporary.")]
        [field: SerializeField] public int Duration { get; private set; }

        [field: SerializeField, HideInInspector] public string UniqueId { get; private set; }

        /// <summary>
        /// Which card players can use this modifier and which card collections can hold its cards.
        /// </summary>
        public abstract ECardTeamAffiliation TeamAffiliation { get; }

        /// <summary>
        /// The action category this modifier belongs to.
        /// </summary>
        public abstract EActionCategory ActionCategory { get; }

        /// <summary>
        /// Creates a runtime state instance representing this modifier.
        /// </summary>
        public abstract ModifierRuntimeState CreateRuntimeState();
    }
}