using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Systems.Tweening.Core;
using Systems.Tweening.Components.System;
using Systems.Tweening.Core.Data.Parameters;

namespace Gameplay.Cards.View
{
    /// <summary>
    /// Base view for any <see cref="Cards.CardController"/>.
    /// Binds to a <see cref="CardModel"/> and listens for <see cref="CardModel.OnCommonDataChanged"/> to redraw visuals.
    /// </summary>
    public abstract class CardView : MonoBehaviour
    {
        private const float DefaultShineGlow = 0f;
        private const float DefaultShineWidth = 0.05f;
        private const float DefaultGlowIntensity = 0f;

        private static readonly int GlowIntensity = Shader.PropertyToID("_Glow");
        private static readonly int ShineGlow = Shader.PropertyToID("_ShineGlow");
        private static readonly int ShineWidth = Shader.PropertyToID("_ShineWidth");

        [Header("Common Card Elements")]
        [SerializeField] protected TMP_Text cardTitleText;
        [SerializeField] protected TMP_Text costText;
        [SerializeField] protected Image cardBacksideImage;
        [SerializeField] protected Image cardFrontsideImage;
        [SerializeField] protected Image cardTypeHighlighterImage;

        [Header("Fade Animation")]
        [Tooltip("Tween group for fade in/out animations.")]
        [SerializeField] private TweenGroup fadeTweenGroup;

        [Tooltip("Canvas group for controlling card transparency.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Grey Out Animation")]
        [SerializeField] private TweenGroup grayOutTweenGroup;

        [Header("Glow Animation")]
        [SerializeField] private TweenData glowTweenData;

        [Header("Shine Animation")]
        [Tooltip("Tween data for the glow intensity of the shine effect. Should finish faster than the width tween.")]
        [SerializeField] private TweenData shineGlowTweenData;

        [Tooltip("Tween data for the width of the shine effect.")]
        [SerializeField] private TweenData shineWidthTweenData;

        [SerializeField] private float targetShineWidth = 1f;
        [SerializeField] private float targetShineGlowIntensity = 1.6f;

        protected CardModel Model;

        private bool _isGrayedOut;

        private TweenBase _glowTween;
        private TweenBase _shineGlowTween;
        private TweenBase _shineWidthTween;
        private Material _shaderBacksideMaterial;
        private Material _shaderFrontsideMaterial;

        private void Start()
        {
            _shaderFrontsideMaterial = new Material(cardFrontsideImage.material);
            cardFrontsideImage.material = _shaderFrontsideMaterial;

            AnimateShine(false, true);
        }

        protected virtual void OnDestroy()
        {
            if (Model != null)
                Model.OnCommonDataChanged -= Refresh;
        }

        /// <summary>
        /// Called once by the controller (Card) after it creates the model.
        /// Sets up data binding and draws initial UI.
        /// </summary>
        public void Initialize(CardModel model, EActionCategory cardCategory)
        {
            if (Model != null)
                Model.OnCommonDataChanged -= Refresh;

            Model = model;

            cardTypeHighlighterImage.sprite = CardTypeHighlighterDictionary.Instance.GetHighlighterSprite(cardCategory);

            if (Model != null)
                Model.OnCommonDataChanged += Refresh;

            Refresh();
        }

        /// <summary>
        /// Enables or disables the glow effect on the card.
        /// </summary>
        /// <param name="state">If <c>true</c>, enables glow; if <c>false</c>, disables glow.</param>
        /// <param name = "glowIntensity">The intensity of the glow when enabled.</param>
        public void SetGlow(bool state, float glowIntensity)
        {
            _glowTween?.Stop();
            float targetValue = state ? glowIntensity : DefaultGlowIntensity;
            _glowTween = TweenFX.FadeFloatTo(
                fromGetter: () => _shaderBacksideMaterial.GetFloat(GlowIntensity),
                setter: f => _shaderBacksideMaterial.SetFloat(GlowIntensity, f),
                targetValue: targetValue,
                data: glowTweenData,
                targetObj: this
            );
        }

        /// <summary>
        /// Plays or reverses the shine animation on the card.
        /// </summary>
        /// <param name="state">If <c>true</c>, plays shine; if <c>false</c>, reverses shine.</param>
        /// <param name = "isInstant">If <c>true</c>, the animation finishes instantly.</param>
        public void AnimateShine(bool state, bool isInstant = false)
        {
            _shineGlowTween?.Stop();
            _shineWidthTween?.Stop();

            float targetGlow = state ? targetShineGlowIntensity : DefaultShineGlow;
            float targetWidth = state ? targetShineWidth : DefaultShineWidth;

            if (isInstant)
            {
                _shaderFrontsideMaterial.SetFloat(ShineGlow, targetGlow);
                _shaderFrontsideMaterial.SetFloat(ShineWidth, targetWidth);
            }
            else
            {
                _shineGlowTween = TweenFX.FadeFloatTo(
                    fromGetter: () => _shaderFrontsideMaterial.GetFloat(ShineGlow),
                    setter: f => _shaderFrontsideMaterial.SetFloat(ShineGlow, f),
                    targetValue: targetGlow,
                    data: shineGlowTweenData,
                    targetObj: this
                );

                _shineWidthTween = TweenFX.FadeFloatTo(
                    fromGetter: () => _shaderFrontsideMaterial.GetFloat(ShineWidth),
                    setter: f => _shaderFrontsideMaterial.SetFloat(ShineWidth, f),
                    targetValue: targetWidth,
                    data: shineWidthTweenData,
                    targetObj: this
                );
            }
        }

        /// <summary>
        /// Changes the material used for the backside image of the card.
        /// </summary>
        public void SetBacksideMaterial(Material cardBacksideMaterial)
        {
            if (cardBacksideMaterial == null)
            {
                CustomLogger.LogWarning("Attempted to set null backside material on card view.", this);
                return;
            }

            _shaderBacksideMaterial = new Material(cardBacksideMaterial);
            cardBacksideImage.material = _shaderBacksideMaterial;

            _shaderBacksideMaterial.SetFloat(GlowIntensity, DefaultGlowIntensity);
        }

        /// <summary>
        /// Fades the card in or out based on the given state.
        /// </summary>
        /// <remarks>
        /// Will not execute if the card is currently grayed out, i.e. unaffordable.
        /// </remarks>
        /// <param name="state">If <c>true</c>, fades out; if <c>false</c>, resets to fully visible.</param>
        public void FadeCard(bool state)
        {
            if (_isGrayedOut)
                return;

            if (state)
            {
                fadeTweenGroup.Play();
            }
            else
            {
                fadeTweenGroup.Stop();
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Grays out the card based on the given state.
        /// </summary>
        /// <param name="state">If <c>true</c>, grays out; if <c>false</c>, resets to normal color.</param>
        public void GrayOut(bool state)
        {
            if (state && !_isGrayedOut)
                grayOutTweenGroup.Play();
            else if (!state && _isGrayedOut)
                grayOutTweenGroup.Reverse();

            _isGrayedOut = state;
        }

        /// <summary>
        /// Changes the sprite used for the backside image of the card.
        /// </summary>
        public void SetBacksideSprite(Sprite cardBacksideSprite)
        {
            if (cardBacksideSprite == null)
            {
                CustomLogger.LogWarning("Attempted to set null backside sprite on card view.", this);
                return;
            }

            cardBacksideImage.sprite = cardBacksideSprite;
        }

        /// <summary>
        /// Called whenever the model fires <see cref="CardModel.OnCommonDataChanged"/> and once on init.
        /// Concrete subclasses can override to add additional data bindings.
        /// </summary>
        protected virtual void Refresh()
        {
            cardTitleText.text = Model.Common.DisplayName;
            costText.text = Model.GetFinalEnergyCost().ToString();
        }
    }
}