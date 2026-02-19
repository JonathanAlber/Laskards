using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier.Data.Runtime;

namespace Gameplay.Cards.Model
{
    /// <summary>
    /// Runtime model for an <see cref="ActionCardController"/>.
    /// Holds both the common data (from <see cref="CardModel"/>) and the action-specific data.
    /// </summary>
    public sealed class ActionCardModel : CardModel
    {
        /// <summary>
        /// The action data associated with this <see cref="ActionCardController"/>
        /// </summary>
        public ModifierRuntimeState ModifierState { get; }

        /// <summary>
        /// Description text for this action card.
        /// </summary>
        public readonly string Description;

        public ActionCardModel(ActionCardDefinition definition) : base(definition)
        {
            ModifierState = definition.Modifier.CreateRuntimeState();
            Description = definition.Description;
        }

        /// <summary>
        /// Cleans up the modifier state on destruction.
        /// </summary>
        public void OnCleanup() => ModifierState.Cleanup();
    }
}