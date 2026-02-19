using Gameplay.Flow;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;
using Systems.MenuManaging;
using Gameplay.StarterDecks.Data;
using Systems.Services;
using Systems.Tweening.Components.System;

namespace UI.Menus.StarterDeck
{
    /// <summary>
    /// Menu allowing the player to select a starter deck at the beginning of the game.
    /// </summary>
    public class StarterDeckSelectionMenu : Menu
    {
        [Header("Starter Deck Selection Menu")]
        [SerializeField] private StarterDeckCollection starterDeckCollection;
        [SerializeField] private StarterDeckDisplay[] starterDeckDisplays;
        [SerializeField] private Transform starterDeckParent;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button overviewMenuConfirmButton;
        [SerializeField] private TweenGroup confirmButtonTweenGroup;
        [SerializeField] private StarterDeckOverviewMenu starterDeckOverviewMenu;

        private StarterDeckDefinition _selectedDeck;
        private StarterDeckDefinition _openedOverviewDeck;

        protected override void Awake()
        {
            base.Awake();

            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            overviewMenuConfirmButton.onClick.AddListener(OnOverviewConfirmButtonClicked);

            StarterDeckDisplay.OnOverviewRequested += HandleOverviewRequested;

            InitializeDisplays();
        }

        protected override void Start()
        {
            base.Start();

            confirmButtonTweenGroup.SetVisibility(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            confirmButton.onClick.RemoveAllListeners();
            overviewMenuConfirmButton.onClick.RemoveAllListeners();

            StarterDeckDisplay.OnOverviewRequested -= HandleOverviewRequested;

            foreach (StarterDeckDisplay display in starterDeckDisplays)
                display.OnSelected -= OnDeckSelected;
        }

        private void InitializeDisplays()
        {
            if (starterDeckCollection.Decks.Count != starterDeckDisplays.Length)
            {
                CustomLogger.LogWarning($"Starter deck count ({starterDeckCollection.Decks.Count}) does not match the " +
                                        $"number of display elements ({starterDeckDisplays.Length}). Some displays" +
                                        " may be inactive.", this);
            }

            for (int i = 0; i < starterDeckCollection.Decks.Count; i++)
            {
                StarterDeckDisplay display = starterDeckDisplays[i];
                if (i < starterDeckDisplays.Length)
                {
                    StarterDeckDefinition deck = starterDeckCollection.Decks[i];
                    display.Initialize(deck);
                    display.OnSelected += OnDeckSelected;
                }
                else
                {
                    display.SetAvailability(false);
                }
            }
        }

        private void HandleOverviewRequested(StarterDeckDefinition starterDeckDefinition)
        {
            _openedOverviewDeck = starterDeckDefinition;
            starterDeckOverviewMenu.Open();
            starterDeckOverviewMenu.SetDeckInfo(_openedOverviewDeck);
        }

        private void OnDeckSelected(StarterDeckDisplay deckDisplay)
        {
            StarterDeckDefinition deckDefinition = deckDisplay.DeckDefinition;
            if (_selectedDeck == deckDefinition)
                return;

            if (_selectedDeck == null)
                confirmButtonTweenGroup.Play();

            foreach (StarterDeckDisplay display in starterDeckDisplays)
                display.SetSelected(display == deckDisplay, display.DeckDefinition == _selectedDeck);

            _selectedDeck = deckDefinition;
        }

        private void OnConfirmButtonClicked()
        {
            if (_selectedDeck == null)
            {
                CustomLogger.LogWarning("Confirm button clicked but no deck has been selected.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                return;

            gameContextService.ChooseStarterDeck(_selectedDeck);
        }

        private void OnOverviewConfirmButtonClicked()
        {
            if (_openedOverviewDeck == null)
            {
                CustomLogger.LogWarning("Overview confirm button clicked but no deck overview is opened.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                return;

            gameContextService.ChooseStarterDeck(_openedOverviewDeck);
        }
    }
}