using System.Collections.Generic;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.StarterDecks.Data;
using Systems.MenuManaging;
using Systems.ObjectPooling;
using Systems.Tweening.Components.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace UI.Menus.StarterDeck
{
    /// <summary>
    /// Menu for displaying an overview of a starter deck.
    /// </summary>
    public class StarterDeckOverviewMenu : Menu
    {
        private const string CardAmountFormat = "{0} Card{1}";

        [Header("Starter Deck Overview Menu")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text cardCountText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TweenGroup confirmButtonTweenGroup;

        [Header("Card Row Settings")]
        [SerializeField] private int cardsPerRow = 6;

        [Header("Object Pooling")]
        [SerializeField] private Transform horizontalLayoutGroupParent;
        [SerializeField] private Transform horizontalLayoutGroupPrefab;
        [SerializeField] private ActionCardController actionCardPrefab;
        [SerializeField] private UnitCardController unitCardPrefab;

        private HashSetObjectPool<Transform> _horizontalLayoutGroupPool;
        private HashSetObjectPool<ActionCardController> _actionCardPool;
        private HashSetObjectPool<UnitCardController> _unitCardPool;

        protected override void Awake()
        {
            base.Awake();

            _actionCardPool = new HashSetObjectPool<ActionCardController>(actionCardPrefab, transform);
            _unitCardPool = new HashSetObjectPool<UnitCardController>(unitCardPrefab, transform);
            _horizontalLayoutGroupPool = new HashSetObjectPool<Transform>(horizontalLayoutGroupPrefab,
                horizontalLayoutGroupParent);
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            confirmButtonTweenGroup.Play();
        }

        /// <summary>
        /// Sets the deck information to be displayed in the overview menu.
        /// </summary>
        /// <param name="deckDefinition">The starter deck definition to display.</param>
        public void SetDeckInfo(StarterDeckDefinition deckDefinition)
        {
            titleText.text = deckDefinition.DisplayName;

            int cardCount = deckDefinition.Cards.Count;
            cardCountText.text = string.Format(CardAmountFormat, cardCount, cardCount == 1 ? "" : "s");

            SetupCardGrid(deckDefinition);
        }

        private void SetupCardGrid(StarterDeckDefinition deckDefinition)
        {
            _horizontalLayoutGroupPool.ReleaseAll();
            _actionCardPool.ReleaseAll();
            _unitCardPool.ReleaseAll();

            scrollRect.verticalNormalizedPosition = 1f;

            int cardsInCurrentRow = 0;
            int rowAmount = 0;
            Transform currentRow = null;

            // 1. Separate by type (Unit, Action)
            List<CardDefinition> unitCards = new(deckDefinition.Cards.Count);
            List<ActionCardDefinition> actionCards = new(deckDefinition.Cards.Count);
            List<CardDefinition> sortedCards = new(deckDefinition.Cards.Count);

            foreach (CardDefinition cardDefinition in deckDefinition.Cards)
            {
                switch (cardDefinition)
                {
                    case UnitCardDefinition:
                        unitCards.Add(cardDefinition);
                        break;
                    case ActionCardDefinition actionCardDef:
                        actionCards.Add(actionCardDef);
                        break;
                }
            }

            // 2. Withing Unit sort by Cost then Name
            unitCards.Sort((a, b) =>
            {
                int costComparison = a.Common.EnergyCost.CompareTo(b.Common.EnergyCost);
                return costComparison != 0
                    ? costComparison
                    : string.CompareOrdinal(a.Common.DisplayName, b.Common.DisplayName);
            });

            // 3. Within Action sort by EActionCategory and then Name and then Cost
            actionCards.Sort((a, b) =>
            {
                int categoryComparison = a.Modifier.ActionCategory.CompareTo(b.Modifier.ActionCategory);
                if (categoryComparison != 0)
                    return categoryComparison;

                int nameComparison = string.CompareOrdinal(a.Common.DisplayName, b.Common.DisplayName);
                return nameComparison != 0
                    ? nameComparison
                    : a.Common.EnergyCost.CompareTo(b.Common.EnergyCost);
            });

            // 4. Combine back into sortedCards
            sortedCards.AddRange(unitCards);
            sortedCards.AddRange(actionCards);

            foreach (CardDefinition cardDefinition in sortedCards)
            {
                if (cardsInCurrentRow == 0)
                {
                    currentRow = _horizontalLayoutGroupPool.Get();
                    currentRow.SetSiblingIndex(rowAmount);
                    rowAmount++;
                }

                Transform cardTransform = null;
                switch (cardDefinition)
                {
                    case ActionCardDefinition actionCardDef:
                    {
                        ActionCardController cardController = _actionCardPool.Get();
                        cardController.Initialize(actionCardDef);
                        cardTransform = cardController.transform;
                        cardTransform.SetParent(currentRow, false);
                        break;
                    }
                    case UnitCardDefinition unitCardDef:
                    {
                        UnitCardController cardController = _unitCardPool.Get();
                        cardController.Initialize(unitCardDef);
                        cardTransform = cardController.transform;
                        cardTransform.SetParent(currentRow, false);
                        break;
                    }
                }

                if (cardTransform != null)
                    cardTransform.SetSiblingIndex(cardsInCurrentRow);
                else
                    CustomLogger.LogError("Card Transform is null for card definition " +
                                          $"{cardDefinition.Common.DisplayName}", this);

                cardsInCurrentRow++;

                if (cardsInCurrentRow >= cardsPerRow)
                    cardsInCurrentRow = 0;
            }
        }
    }
}