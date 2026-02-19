using TMPro;
using UnityEngine;

namespace Gameplay.Player.UI
{
    /// <summary>
    /// Displays the player's current drawable card amount in the UI.
    /// </summary>
    public class PlayerDrawAmountDisplay : MonoBehaviour
    {
        [Tooltip("Text component to display the player's current drawable card amount.")]
        [SerializeField] private TMP_Text drawAmountText;

        private void OnEnable() => PlayerController.OnDrawableCardAmountChanged += HandleDrawAmountChanged;

        private void OnDisable() => PlayerController.OnDrawableCardAmountChanged -= HandleDrawAmountChanged;

        private void HandleDrawAmountChanged(int newDrawAmount) => drawAmountText.text = newDrawAmount.ToString();
    }
}