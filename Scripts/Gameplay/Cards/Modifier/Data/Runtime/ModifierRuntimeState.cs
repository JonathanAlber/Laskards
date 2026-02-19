using System;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Effects.Display;
using Utility.Logging;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Base runtime representation of <see cref="ModifierData"/>. Holds mutable runtime values.
    /// </summary>
    [Serializable]
    public abstract class ModifierRuntimeState : IModifier
    {
        /// <summary>
        /// The action category this modifier belongs to.
        /// </summary>
        public EActionCategory ActionCategory { get; }

        /// <summary>
        /// The duration type of the modifier.
        /// </summary>
        public EDurationType DurationType { get; }

        /// <summary>
        /// Duration in turns. Copied from the <see cref="ModifierData"/> initially,
        /// but can be changed at runtime.
        /// </summary>
        public int Duration { get; protected set; }

        /// <summary>
        /// The effect display information for UI representation.
        /// </summary>
        public EffectData EffectData { get; }

        /// <summary>
        /// The source modifier data this runtime state is based on.
        /// </summary>
        protected readonly string UniqueID;

        protected ModifierRuntimeState(ModifierData data)
        {
            UniqueID = data.UniqueId;
            ActionCategory = data.ActionCategory;
            DurationType = data.DurationType;
            Duration = data.Duration;
            EffectData = data.EffectData;
        }

        public virtual bool Validate(IModifiableBase target)
        {
            if (target != null)
                return true;

            CustomLogger.LogWarning($"Modifier '{EffectData.DisplayName}' received a null target.", null);
            return false;
        }

        public abstract bool TryCreateEffect(IModifiableBase target, ETeam creatorTeam, out IEffect effect);

        public virtual void Execute() { }

        public virtual void Cleanup() { }

        /// <summary>
        /// Creates a shallow copy of the current runtime state.
        /// </summary>
        /// <remarks>
        /// If a derived class adds mutable reference-type fields that require
        /// deep copying, override this method and implement custom clone logic.
        /// </remarks>
        public virtual ModifierRuntimeState Clone() => (ModifierRuntimeState)MemberwiseClone();
    }
}