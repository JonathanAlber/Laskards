using TMPro;
using UnityEngine;

namespace Gameplay.Player.UI
{
    /// <summary>
    /// Displays the player's next turn energy preview in the UI.
    /// </summary>
    public class PlayerEnergyPreviewDisplay : MonoBehaviour
    {
        private const string TextFormat = "+{0}";

        [SerializeField] private TMP_Text previewText;

        private void OnEnable() => PlayerController.OnGainedEnergyNextTurnChanged += HandleEnergyPreviewChanged;

        private void OnDisable() => PlayerController.OnGainedEnergyNextTurnChanged -= HandleEnergyPreviewChanged;

        private void HandleEnergyPreviewChanged(int nextGain) => previewText.text = string.Format(TextFormat, nextGain);
    }
}