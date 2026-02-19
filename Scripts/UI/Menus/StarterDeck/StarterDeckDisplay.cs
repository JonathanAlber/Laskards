using System;
using Gameplay.StarterDecks.Data;
using Systems.Tweening.Components.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menus.StarterDeck
{
    /// <summary>
    /// UI component for displaying a starter deck option to the player.
    /// </summary>
    public class StarterDeckDisplay : MonoBehaviour
    {
        private const string CardAmountFormat = "{0} Card{1}";

        /// <summary>
        /// Event triggered when an overview of this starter deck is requested.
        /// </summary>
        public static event Action<StarterDeckDefinition> OnOverviewRequested;

        /// <summary>
        /// Event triggered when this starter deck display is selected.
        /// </summary>
        public event Action<StarterDeckDisplay> OnSelected;

        [SerializeField] private TMP_Text deckTitle;
        [SerializeField] private TMP_Text cardAmountText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image birdImage;
        [SerializeField] private Button selectionButton;
        [SerializeField] private Button seeOverviewButton;
        [SerializeField] private GameObject availableGroup;
        [SerializeField] private GameObject unavailableGroup;
        [SerializeField] private TweenGroup selectionTweenGroup;

        public StarterDeckDefinition DeckDefinition { get; private set; }

        private void OnDestroy()
        {
            selectionButton.onClick.RemoveListener(OnSelectionButtonClicked);
            seeOverviewButton.onClick.RemoveListener(OnSeeOverviewButtonClicked);
        }

        /// <summary>
        /// Initializes the display with the given starter deck definition.
        /// </summary>
        public void Initialize(StarterDeckDefinition deckDefinition)
        {
            DeckDefinition = deckDefinition;
            deckTitle.text = DeckDefinition.DisplayName;

            int cardCount = DeckDefinition.Cards.Count;
            cardAmountText.text = string.Format(CardAmountFormat, cardCount, cardCount == 1 ? "" : "s");

            descriptionText.text = DeckDefinition.Description;

            birdImage.sprite = DeckDefinition.BirdImage;

            selectionButton.onClick.AddListener(OnSelectionButtonClicked);
            seeOverviewButton.onClick.AddListener(OnSeeOverviewButtonClicked);
        }

        /// <summary>
        /// Activates or deactivates the selection visual state.
        /// </summary>
        /// <param name="isSelected"><c>true</c> to select, <c>false</c> to deselect.</param>
        /// <param name="wasSelected"><c>true</c> if it was previously selected, <c>false</c> otherwise.</param>
        public void SetSelected(bool isSelected, bool wasSelected)
        {
            if (isSelected)
            {
                selectionTweenGroup.Play();
            }
            else
            {
                if (wasSelected)
                    selectionTweenGroup.Reverse();
                else
                    selectionTweenGroup.SetVisibility(false);
            }
        }

        /// <summary>
        /// Sets the visibility of the display based on whether the boss is available for selection.
        /// </summary>
        /// <param name="isAvailable"><c>true</c> if the boss data is available; otherwise, <c>false</c>.</param>
        public void SetAvailability(bool isAvailable)
        {
            if (isAvailable)
            {
                availableGroup.SetActive(true);
                unavailableGroup.SetActive(false);
            }
            else
            {
                availableGroup.SetActive(false);
                unavailableGroup.SetActive(true);
            }
        }

        private void OnSelectionButtonClicked() => OnSelected?.Invoke(this);

        private void OnSeeOverviewButtonClicked() => OnOverviewRequested?.Invoke(DeckDefinition);
    }
}