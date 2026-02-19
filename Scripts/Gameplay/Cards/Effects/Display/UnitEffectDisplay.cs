using Systems.Tweening.Core;
using Systems.Tweening.Core.Data.Parameters;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// UI component for displaying an effect.
    /// </summary>
    public sealed class UnitEffectDisplay : EffectDisplay
    {
        private const float DefaultGlowIntensity = 0f;

        private static readonly int GlowIntensity = Shader.PropertyToID("_Glow");
        private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

        [Header("Effect Glow Highlight")]
        [Tooltip("Intensity of the glow highlight effect when active.")]
        [SerializeField] private float glowHighlightIntensity = 25f;

        [Tooltip("Intensity of the glow highlight effect when active.")]
        [SerializeField] private Color glowHighlightColor = Color.blue;

        [Tooltip("Intensity of the glow highlight effect when active.")]
        [SerializeField] private Color glowSelectionColor = Color.red;

        [Tooltip("Tween data for the glow highlight effect.")]
        [SerializeField] private TweenData glowHighlightIntensityTweenData;

        [Tooltip("Tween data for the glow highlight effect.")]
        [SerializeField] private TweenData glowHighlightColorTweenData;

        [Tooltip("Image component for the glowing background.")]
        [SerializeField] private Image glowingBackgroundImage;

        private EHighlightMode _mode;
        private TweenBase _glowTween;
        private TweenBase _colorTween;
        private Material _glowMaterial;

        private void Awake()
        {
            _glowMaterial = Instantiate(glowingBackgroundImage.material);
            glowingBackgroundImage.material = _glowMaterial;

            _glowMaterial.SetFloat(GlowIntensity, DefaultGlowIntensity);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClearGlowTween();

            if (_glowMaterial != null)
                Destroy(_glowMaterial);
        }

        /// <summary>
        /// Sets the highlight state of the effect display.
        /// </summary>
        public void SetHighlight(EHighlightMode mode)
        {
            if (HasEffect)
                return;

            if (_mode == mode)
                return;

            _mode = mode;

            float targetIntensity;
            Color targetColor;

            switch (mode)
            {
                case EHighlightMode.None:
                    targetIntensity = DefaultGlowIntensity;
                    targetColor = glowHighlightColor;
                    break;

                case EHighlightMode.ValidTarget:
                    targetIntensity = glowHighlightIntensity;
                    targetColor = glowHighlightColor;
                    break;

                case EHighlightMode.HoveredTarget:
                    targetIntensity = glowHighlightIntensity;
                    targetColor = glowSelectionColor;
                    break;

                default:
                    targetIntensity = DefaultGlowIntensity;
                    targetColor = glowHighlightColor;
                    break;
            }

            // Intensity tween
            _glowTween?.Stop();
            _glowTween = TweenFX.FadeFloatTo(
                fromGetter: () => _glowMaterial.GetFloat(GlowIntensity),
                setter: value => _glowMaterial.SetFloat(GlowIntensity, value),
                targetValue: targetIntensity,
                data: glowHighlightIntensityTweenData,
                targetObj: this
            );
            _glowTween.OnComplete += ClearGlowTween;

            // Color tween
            _colorTween?.Stop();
            _colorTween = TweenFX.ColorTo(
                fromGetter: () => _glowMaterial.GetColor(GlowColor),
                setter: color => _glowMaterial.SetColor(GlowColor, color),
                targetValue: targetColor,
                data: glowHighlightColorTweenData,
                targetObj: this
            );
            _colorTween.OnComplete += ClearColorTween;
        }

        private void ClearGlowTween(TweenBase _ = null)
        {
            if (_glowTween == null)
                return;

            _glowTween.OnComplete -= ClearGlowTween;
            _glowTween = null;
        }

        private void ClearColorTween(TweenBase _ = null)
        {
            if (_colorTween == null)
                return;

            _colorTween.OnComplete -= ClearColorTween;
            _colorTween = null;
        }
    }
}