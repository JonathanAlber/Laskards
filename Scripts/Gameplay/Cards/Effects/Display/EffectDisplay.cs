using Gameplay.Cards.Data;
using Systems.Tooltip;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// Parent class for UI components that display effects.
    /// </summary>
    public class EffectDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image effectIcon;

        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected TooltipTrigger tooltipTrigger;

        [Header("Effect Icons")]
        [SerializeField] private Sprite permanentEffectBackground;
        [SerializeField] private Sprite temporaryEffectBackground;

        /// <summary>
        /// The effect data currently displayed.
        /// </summary>
        public EffectData EffectData { get; private set; }

        /// <summary>
        /// Indicates whether an effect is currently displayed.
        /// </summary>
        public bool HasEffect { get; private set; }

        private IEffect _effect;
        private EffectTooltipBinding _tooltipBinding;

        protected virtual void OnDestroy() => _tooltipBinding?.Dispose();

        /// <summary>
        /// Sets the effect display with the given icon and tooltip text.
        /// </summary>
        public virtual void SetEffect(IEffect effect)
        {
            if (effect == null)
            {
                CustomLogger.LogError("Attempted to set null effect on EffectDisplay.", this);
                return;
            }

            _effect = effect;
            EffectData = _effect.EffectData;

            if (EffectData.Icon == null)
                CustomLogger.LogWarning($"Icon for effect '{EffectData.DisplayName}' is null. Using default icon.",
                    this);
            else
                SetIcon(EffectData.Icon);

            SetComponentsEnabled(true);
            SetBackground(_effect.DurationType == EDurationType.Permanent);

            _tooltipBinding = new EffectTooltipBinding(tooltipTrigger, EffectData.Description, _effect.TooltipData);

            HasEffect = true;
        }

        /// <summary>
        /// Clears the effect display.
        /// </summary>
        public virtual void ClearEffect()
        {
            effectIcon.sprite = null;
            SetBackground(true);
            SetComponentsEnabled(false);

            tooltipTrigger.SetText(string.Empty);
            _tooltipBinding?.Dispose();
            _tooltipBinding = null;

            EffectData = null;
            _effect = null;

            HasEffect = false;
        }

        /// <summary>
        /// Enables or disables the effect display components.
        /// </summary>
        public void SetComponentsEnabled(bool state)
        {
            effectIcon.enabled = state;
            tooltipTrigger.enabled = state;
        }

        private void SetIcon(Sprite icon) => effectIcon.sprite = icon;

        private void SetBackground(bool isPermanent)
        {
            backgroundImage.sprite = isPermanent
                ? permanentEffectBackground
                : temporaryEffectBackground;
        }
    }
}