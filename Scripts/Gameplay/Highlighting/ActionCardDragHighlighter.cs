using UnityEngine;
using Gameplay.Cards;
using Gameplay.Cards.Data;
using Gameplay.Cards.Modifier;
using System.Collections.Generic;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Base class for handling visualization for dragging action cards that target modifiable entities,
    /// such as highlighting valid targets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ActionCardDragHighlighter<T> : CardDragHighlighter where T : IModifiableBase
    {
        [Header("Action Card Drag Highlighter")]
        [Tooltip("The action category this highlighter is responsible for.")]
        [SerializeField] protected EActionCategory actionCategory;

        protected readonly List<T> ValidModifiables = new();

        protected override bool ValidateCard(CardController card)
        {
            if (card is not ActionCardController actionCard)
                return false;

            return actionCard.TypedModel.ModifierState.ActionCategory == actionCategory;
        }
    }
}