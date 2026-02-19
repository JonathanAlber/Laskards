using System;
using System.Collections.Generic;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// Provides key-value pairs used inside tooltip templates.
    /// Implement this on runtime-state objects or effect instances.
    /// </summary>
    public interface IEffectTooltipData
    {
        /// <summary>
        /// Raised when tooltip values change.
        /// </summary>
        event Action OnRemainingDurationChanged;

        /// <summary>
        /// The display name of the effect or modifier.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Returns a read-only dictionary of tooltip variables.
        /// Keys should match placeholders in descriptions (e.g. "Duration", "Damage").
        /// </summary>
        IReadOnlyDictionary<string, object> GetTooltipValues();
    }
}