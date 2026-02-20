using System;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Cards.Modifier;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents a status effect that can be applied to a modifiable target,
    /// with a defined duration and lifecycle methods.
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// Raised when tooltip values change.
        /// </summary>
        event Action OnExpired;

        /// <summary>
        /// The target that this effect is applied to.
        /// </summary>
        IModifiableBase Target { get; }

        /// <summary>
        /// The team of the creator of the effect.
        /// </summary>
        ETeam CreatorTeam { get; }

        /// <summary>
        /// The duration type of the effect (e.g., temporary, permanent).
        /// </summary>
        EDurationType DurationType { get; }

        /// <summary>
        /// Remaining duration of the effect.
        /// Only meaningful for temporary effects.
        /// </summary>
        int RemainingDuration { get; }

        /// <summary>
        /// The effect data for UI representation.
        /// </summary>
        EffectData EffectData { get; }

        /// <summary>
        /// The tooltip data associated with this effect.
        /// </summary>
        IEffectTooltipData TooltipData { get; }

        /// <summary>
        /// Indicates whether the effect has expired.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// Applies the effect to the target.
        /// </summary>
        void Apply();

        /// <summary>
        /// Advances the effect's duration by one tick (e.g., one turn).
        /// </summary>
        void Tick();

        /// <summary>
        /// Reverts the effect, removing its impact from the target.
        /// </summary>
        void Revert();

        /// <summary>
        /// Sets a new duration for the modifier.
        /// </summary>
        void SetDuration(int newDuration);

        /// <summary>
        /// Called when the duration of the modifier changes.
        /// </summary>
        void OnDurationChanged();
    }
}