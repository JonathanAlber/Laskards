using Systems.Tweening.Components.System;
using TMPro;
using UnityEngine;

namespace Gameplay.Player.UI
{
    /// <summary>
    /// Displays the player's current energy amount in the UI.
    /// </summary>
    public class PlayerEnergyAmountDisplay : MonoBehaviour
    {
        [Tooltip("Text component to display the player's current energy amount.")]
        [SerializeField] private TMP_Text energyAmountText;

        [Tooltip("Tween group to play when the player tries to spend more energy than they have.")]
        [SerializeField] private TweenGroup notEnoughEnergyTweenGroup;

        private void OnEnable()
        {
            PlayerController.OnEnergyChanged += HandleEnergyChanged;
            PlayerController.OnNotEnoughEnergy += HandleNotEnoughEnergy;
        }

        private void OnDisable()
        {
            PlayerController.OnEnergyChanged -= HandleEnergyChanged;
            PlayerController.OnNotEnoughEnergy -= HandleNotEnoughEnergy;
        }

        private void HandleNotEnoughEnergy() => notEnoughEnergyTweenGroup.Play();

        private void HandleEnergyChanged(int newEnergy) => energyAmountText.text = newEnergy.ToString();
    }
}