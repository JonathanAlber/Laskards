using TMPro;
using UnityEngine;

namespace Gameplay.Player.UI
{
    /// <summary>
    /// Displays the player's next turn energy preview in the UI.
    /// </summary>
    public class PlayerDrawableCardAmountPreviewDisplay : MonoBehaviour
    {
        private const string TextFormat = "+{0}";

        [SerializeField] private TMP_Text previewText;

        private void OnEnable() => PlayerController.OnDrawableCardAmountNextTurnChanged += HandleEnergyPreviewChanged;

        private void OnDisable() => PlayerController.OnDrawableCardAmountNextTurnChanged -= HandleEnergyPreviewChanged;

        private void HandleEnergyPreviewChanged(int nextGain) => previewText.text = string.Format(TextFormat, nextGain);
    }
}