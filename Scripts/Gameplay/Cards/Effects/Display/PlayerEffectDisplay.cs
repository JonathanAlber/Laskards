using VFX;
using System;
using Utility;
using UnityEngine;
using Systems.Services;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// UI component for displaying a player effect.
    /// </summary>
    public class PlayerEffectDisplay : EffectDisplay
    {
        private const float DefaultGlowIntensity = 0f;

        /// <summary>
        /// Event invoked when the boss is damaged by an effect.
        /// </summary>
        public static event Action<EffectDisplay> OnBossDamagedByEffect;

        private static readonly int GlowIntensity = Shader.PropertyToID("_Glow");

        [Header("Player Effect Display")]
        [SerializeField] private float targetGlowIntensity = 10f;

        private PlayerEffect _playerEffect;
        private Material _backgroundMaterial;

        private void Awake()
        {
            _backgroundMaterial = Instantiate(backgroundImage.material);
            backgroundImage.material = _backgroundMaterial;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_playerEffect != null)
                _playerEffect.OnDamagedBoss -= HandleBossDamaged;
        }

        public override void SetEffect(IEffect effect)
        {
            base.SetEffect(effect);

            if (effect is not PlayerEffect playerEffect)
                return;

            _playerEffect = playerEffect;
            _playerEffect.OnDamagedBoss += HandleBossDamaged;
        }

        public override void ClearEffect()
        {
            base.ClearEffect();

            if (_playerEffect != null)
                _playerEffect.OnDamagedBoss -= HandleBossDamaged;
        }

        /// <summary>
        /// Sets the active state of the effect display.
        /// </summary>
        /// <param name="active"><c>true</c> to activate, <c>false</c> to deactivate.</param>
        public void SetActive(bool active) => gameObject.SetActive(active);

        private void SetGlowing(bool glowing)
        {
            _backgroundMaterial.SetFloat(GlowIntensity, glowing ? targetGlowIntensity : DefaultGlowIntensity);
        }

        private void HandleBossDamaged()
        {
            OnBossDamagedByEffect?.Invoke(this);

            SetGlowing(true);

            if (!ServiceLocator.TryGet(out ParticleTweenDataProvider provider))
                return;

            float delay = provider.Config.EffectDamageTweenData.Duration + provider.Config.EffectDamageTweenData.Delay;
            CoroutineRunner.Instance.RunAfterSeconds(() => SetGlowing(false), delay);
        }
    }
}