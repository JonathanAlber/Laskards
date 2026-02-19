using Systems.Services;
using UnityEngine;
using Utility.Logging;
using VFX.Data;

namespace VFX
{
    /// <summary>
    /// Provides access to particle tween configuration data.
    /// </summary>
    public class ParticleTweenDataProvider : GameServiceBehaviour
    {
        /// <summary>
        /// Configuration data for particle tween effects.
        /// </summary>
        [field: SerializeField] public ParticleTweenDataConfig Config { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (Config == null)
                CustomLogger.LogWarning($"{nameof(ParticleTweenDataConfig)} is not assigned.", this);
        }
    }
}