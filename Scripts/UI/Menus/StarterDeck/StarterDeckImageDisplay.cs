using Gameplay.Flow;
using Gameplay.StarterDecks.Data;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace UI.Menus.StarterDeck
{
    /// <summary>
    /// Displays the image of the chosen starter deck's bird.
    /// </summary>
    public class StarterDeckImageDisplay : MonoBehaviour
    {
        [SerializeField] private Image starterDeckImage;

        private void Awake() => GameContextService.OnStarterDeckChosen += HandleStarterDeckChosen;

        private void OnDestroy() => GameContextService.OnStarterDeckChosen -= HandleStarterDeckChosen;

        private void HandleStarterDeckChosen(StarterDeckDefinition starterDeckDefinition)
        {
            if (starterDeckDefinition == null)
            {
                CustomLogger.LogWarning("Starter deck definition is null.", this);
                return;
            }

            starterDeckImage.sprite = starterDeckDefinition.BirdImage;
        }
    }
}