using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Utility.Logging;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Generic base that provides strongly-typed access to both the modifier data and the target type.
    /// Adds type-safe validation and effect creation while keeping external APIs non-generic.
    /// </summary>
    /// <typeparam name="TData">The type of the backing <see cref="ModifierData"/>.</typeparam>
    /// <typeparam name="TTarget">The type of the modifiable target this modifier can be applied to.</typeparam>
    public abstract class TypedModifierRuntimeState<TData, TTarget> : ModifierRuntimeState
        where TData : ModifierData
        where TTarget : IModifiableBase
    {
        protected TypedModifierRuntimeState(TData data) : base(data) { }

        public sealed override bool Validate(IModifiableBase target)
        {
            if (target is TTarget typedTarget)
                return Validate(typedTarget);

            CustomLogger.LogWarning(
                $"Modifier '{EffectData.DisplayName}' expected target of type '{typeof(TTarget).Name}' " +
                $"but received '{target?.GetType().Name ?? "null"}'.", null);
            return false;
        }

        public sealed override bool TryCreateEffect(IModifiableBase target, ETeam creatorTeam, out IEffect effect)
        {
            if (target is TTarget typedTarget)
            {
                effect = CreateEffect(typedTarget, creatorTeam);
                if (effect != null)
                    return true;

                CustomLogger.LogError($"Modifier '{EffectData.DisplayName}' created a null effect for target" +
                                      $" '{typeof(TTarget).Name}'.", null);
                return false;
            }

            CustomLogger.LogError(
                $"Modifier '{EffectData.DisplayName}' failed to create effect because target type " +
                $"'{target?.GetType().Name ?? "null"}' is not assignable" +
                $" to '{typeof(TTarget).Name}'.",
                null);

            effect = null;
            return false;
        }

        /// <summary>
        /// Performs typed validation for the modifier's specific target type.
        /// </summary>
        /// <param name="target">The typed target instance.</param>
        /// <returns>
        /// <c>true</c> if the target is valid; otherwise, <c>false</c>.
        /// Default implementation always returns <c>true</c>.
        /// Override this method in derived classes to add custom validation logic.
        /// </returns>
        protected virtual bool Validate(TTarget target) => true;

        /// <summary>
        /// Strongly-typed effect creation hook for derived classes.
        /// </summary>
        /// <param name="target">The strongly-typed target instance.</param>
        /// <param name = "creatorTeam">The team that created the effect.</param>
        /// <returns>The created effect instance.</returns>
        protected abstract IEffect CreateEffect(TTarget target, ETeam creatorTeam);
    }
}