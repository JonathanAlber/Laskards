using System;
using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Cards.Modifier;
using UnityEngine;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Base class for effects that can be applied to modifiable entities.
    /// </summary>
    /// <typeparam name="TTarget">The type of modifiable entity the effect is applied to.</typeparam>
    public abstract class Effect<TTarget> : IEffectTooltipData, IEffect where TTarget : IModifiableBase
    {
        /// <summary>
        /// Event invoked when tooltip values change.
        /// </summary>
        public event Action OnRemainingDurationChanged;

        /// <summary>
        /// Event invoked when the effect expires.
        /// </summary>
        public event Action OnExpired;

        public ETeam CreatorTeam { get; }

        public EDurationType DurationType { get; }

        /// <summary>
        /// The target that this effect is applied to.
        /// </summary>
        protected TTarget Target { get; }
        IModifiableBase IEffect.Target => Target;

        public int RemainingDuration { get; private set; }

        public EffectData EffectData { get; }

        public IEffectTooltipData TooltipData => this;

        public bool IsExpired => DurationType == EDurationType.Temporary && RemainingDuration <= 0;

        public string DisplayName => EffectData.DisplayName;

        protected Effect(TTarget target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData)
        {
            Target = target;
            CreatorTeam = creatorTeam;
            DurationType = durationType;
            RemainingDuration = duration;
            EffectData = effectData;
        }

        public virtual void Apply() => OnApply();

        /// <summary>
        /// Called when the effect is applied to the target.
        /// </summary>
        protected virtual void OnApply() { }

        public virtual void Tick()
        {
            if (DurationType != EDurationType.Temporary)
                return;

            SetDuration(RemainingDuration - 1);
        }

        public virtual void Revert() => OnRevert();

        public void SetDuration(int newDuration)
        {
            if (DurationType != EDurationType.Temporary)
                return;

            if (newDuration < 0)
                newDuration = 0;

            if (Mathf.Approximately(RemainingDuration, newDuration))
                return;

            RemainingDuration = newDuration;

            OnDurationChanged();

            OnRemainingDurationChanged?.Invoke();

            if (IsExpired)
                OnExpired?.Invoke();
        }

        public virtual void OnDurationChanged() { }

        public virtual IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "Duration", RemainingDuration }
            };
        }

        /// <summary>
        /// Creates a shallow copy of this effect instance.
        /// Use this for safe simulation or previewing future state.
        /// </summary>
        /// <returns>A shallow copy of this effect.</returns>
        public Effect<TTarget> Clone() => (Effect<TTarget>)MemberwiseClone();

        /// <summary>
        /// Called when the effect is reverted from the target.
        /// </summary>
        protected virtual void OnRevert() { }
    }
}