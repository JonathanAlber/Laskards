using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;
using Systems.Tooltip;
using Systems.Tweening.Core;
using System.Collections.Generic;
using Systems.ObjectPooling.Gameplay;
using Systems.Tweening.Components.System;
using Systems.Tweening.Core.Data.Parameters;

namespace Gameplay.Units
{
    /// <summary>
    /// Visual representation of a unit. Handles only UI updates.
    /// </summary>
    public sealed class UnitView : MonoBehaviour
    {
        private const float DefaultFadeAmount = -0.1f;

        private static readonly int FadeAmount = Shader.PropertyToID("_FadeAmount");

        [Header("UI References")]
        [Tooltip("Image component that renders the unit sprite.")]
        [SerializeField] private Image image;

        [Tooltip("Text component that displays the unit's attack damage.")]
        [SerializeField] private TMP_Text attackDamageText;

        [Header("Remaining Moves")]
        [Tooltip("Tooltip text format for remaining moves display. " +
                 "The {0} token will be replaced with the remaining moves count.")]
        [SerializeField] private string remainingMovesTooltipTextFormat = "{0} Remaining Moves";
        [SerializeField] private string unlimitedRemainingMovesTooltipText = "Unlimited Moves";

        [Tooltip("The transform container for remaining moves UI elements.")]
        [SerializeField] private Transform remainingMovesContainer;

        [Tooltip("Tooltip trigger for remaining moves display.")]
        [SerializeField] private TooltipTrigger remainingMovesTooltip;

        [Header("Remaining Lifetime")]
        [Tooltip("Tooltip text format for remaining lifetime display. " +
                 "The {0} token will be replaced with the remaining moves count.")]
        [SerializeField] private string remainingLifetimeTooltipTextFormat = "{0} Remaining Lifetime";
        [SerializeField] private string unlimitedRemainingLifetimeTooltipText = "Unlimited Lifetime";

        [Tooltip("The transform container for remaining lifetime UI elements.")]
        [SerializeField] private Transform remainingLifetimeContainer;

        [Tooltip("Tooltip trigger for remaining lifetime display.")]
        [SerializeField] private TooltipTrigger remainingLifetimeTooltip;

        [Header("Fade Out Animation")]

        [Tooltip("The images to be faded out.")]
        [SerializeField] private Image[] imagesToFade;

        [Tooltip("The target value for the fade out animation.")]
        [SerializeField] private float targetIntensity = 1f;

        [Tooltip("Tween data configuring the fade out animation.")]
        [SerializeField] private TweenData fadeOutTweenData;

        [Tooltip("Tween group to fade out elements that can not have the fade material.")]
        [SerializeField] private TweenGroup fadeTweenGroup;

        private readonly List<GameObject> _remainingMovesImages = new();

        private UnitModel _model;
        private bool _fadeOutStarted;

        private void Awake()
        {
            foreach (Image img in imagesToFade)
            {
                Material fadeMat = Instantiate(img.material);
                fadeMat.SetFloat(FadeAmount, DefaultFadeAmount);
                img.material = fadeMat;
            }
        }

        private void OnDestroy()
        {
            if (_model == null)
                return;

            _model.OnDamageChanged -= UpdateDamageDisplay;
            _model.OnRemainingMoveCountChanged -= UpdateRemainingMovesDisplay;
            _model.OnRemainingLifetimeChanged -= UpdateRemainingLifetimeDisplay;
        }

        /// <summary>
        /// Initializes the unit view with the specified unit type.
        /// </summary>
        public void Initialize(UnitModel unitModel)
        {
            if (unitModel == null)
            {
                CustomLogger.LogError("Cannot initialize UnitView with null UnitModel.", this);
                return;
            }

            _model = unitModel;

            _model.OnDamageChanged += UpdateDamageDisplay;
            _model.OnRemainingMoveCountChanged += UpdateRemainingMovesDisplay;
            _model.OnRemainingLifetimeChanged += UpdateRemainingLifetimeDisplay;

            Sprite visual = UnitSpriteManager.Instance.GetUnitIcon(_model.UnitType, _model.Team);
            ApplyVisual(visual);

            UpdateRemainingMovesDisplay(_model.RemainingMoves);
            UpdateDamageDisplay(_model.GetFinalDamage());
            UpdateRemainingLifetimeDisplay(_model.Lifetime);
        }

        /// <summary>
        /// Starts the fade out animation for the unit view.
        /// This animation is final and cannot be reversed.
        /// </summary>
        public void StartFadeOutAnimation()
        {
            if (_fadeOutStarted)
                return;

            _fadeOutStarted = true;

            fadeTweenGroup.Play();

            foreach (Image img in imagesToFade)
            {
                Material fadeMaterial = img.material;
                TweenFX.FadeFloatTo(
                    fromGetter: () => fadeMaterial.GetFloat(FadeAmount),
                    setter: value => fadeMaterial.SetFloat(FadeAmount, value),
                    targetValue: targetIntensity,
                    data: fadeOutTweenData,
                    targetObj: this
                );
            }
        }

        private void UpdateRemainingLifetimeDisplay(int remainingLifetime)
        {
            // -1 indicates unlimited lifetime
            if (remainingLifetime == -1)
            {
                switch (remainingLifetimeContainer.childCount)
                {
                    case 0:
                    {
                        GameObject lifetimeImage = RemainingMovesImagePool.Instance.Get();
                        lifetimeImage.transform.SetParent(remainingLifetimeContainer, false);
                        break;
                    }
                    case > 1:
                    {
                        for (int i = remainingLifetimeContainer.childCount - 1; i >= 1; i--)
                            RemainingMovesImagePool.Instance.Release(remainingLifetimeContainer.GetChild(i).gameObject);

                        break;
                    }
                }

                remainingLifetimeTooltip.SetText(unlimitedRemainingLifetimeTooltipText);
                return;
            }

            // Adjust the number of lifetime images to match remaining lifetime
            if (remainingLifetimeContainer.childCount > remainingLifetime)
            {
                for (int i = remainingLifetimeContainer.childCount - 1; i >= remainingLifetime; i--)
                    RemainingMovesImagePool.Instance.Release(remainingLifetimeContainer.GetChild(i).gameObject);
            }
            else if (remainingLifetimeContainer.childCount < remainingLifetime)
            {
                int imagesToAdd = remainingLifetime - remainingLifetimeContainer.childCount;
                for (int i = 0; i < imagesToAdd; i++)
                {
                    GameObject lifetimeImage = RemainingMovesImagePool.Instance.Get();
                    lifetimeImage.transform.SetParent(remainingLifetimeContainer, false);
                }
            }

            string tooltipText = string.Format(remainingLifetimeTooltipTextFormat, remainingLifetime);
            remainingLifetimeTooltip.SetText(tooltipText);
        }

        private void UpdateRemainingMovesDisplay(int remainingMoves)
        {
            // -1 indicates unlimited moves
            if (remainingMoves == -1)
            {
                switch (_remainingMovesImages.Count)
                {
                    case 0:
                    {
                        GameObject moveImage = RemainingMovesImagePool.Instance.Get();
                        moveImage.transform.SetParent(remainingMovesContainer, false);
                        _remainingMovesImages.Add(moveImage);
                        break;
                    }
                    case > 1:
                    {
                        for (int i = _remainingMovesImages.Count - 1; i >= 1; i--)
                        {
                            RemainingMovesImagePool.Instance.Release(_remainingMovesImages[i]);
                            _remainingMovesImages.RemoveAt(i);
                        }

                        break;
                    }
                }

                remainingMovesTooltip.SetText(unlimitedRemainingMovesTooltipText);
                return;
            }

            if (_remainingMovesImages.Count > remainingMoves)
            {
                for (int i = _remainingMovesImages.Count - 1; i >= remainingMoves; i--)
                {
                    RemainingMovesImagePool.Instance.Release(_remainingMovesImages[i]);
                    _remainingMovesImages.RemoveAt(i);
                }
            }
            else if (_remainingMovesImages.Count < remainingMoves)
            {
                int imagesToAdd = remainingMoves - _remainingMovesImages.Count;
                for (int i = 0; i < imagesToAdd; i++)
                {
                    GameObject moveImage = RemainingMovesImagePool.Instance.Get();
                    moveImage.transform.SetParent(remainingMovesContainer, false);
                    _remainingMovesImages.Add(moveImage);
                }
            }

            string tooltipText = string.Format(remainingMovesTooltipTextFormat, remainingMoves);
            remainingMovesTooltip.SetText(tooltipText);
        }

        private void ApplyVisual(Sprite visual)
        {
            if (visual == null)
            {
                CustomLogger.LogWarning("Attempted to apply null visual.", this);
                return;
            }

            image.sprite = visual;
        }

        private void UpdateDamageDisplay(int damage) => attackDamageText.text = damage.ToString();
    }
}