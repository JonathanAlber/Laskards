using Framework;
using Gameplay.Cards.Data;
using UnityEngine;
using Utility.Collections;
using Utility.Logging;

namespace Gameplay.Cards.View
{
    /// <summary>
    /// Dictionary mapping card action categories to their corresponding highlighter sprites.
    /// </summary>
    public class CardTypeHighlighterDictionary : CustomSingleton<CardTypeHighlighterDictionary>
    {
        [Tooltip("Mapping of action categories to their corresponding highlighter sprites.")]
        [SerializeField] private SerializableDictionary< EActionCategory, Sprite > categoryToHighlighterSprite = new();

        /// <summary>
        /// Gets the highlighter sprite for the specified action category.
        /// </summary>
        /// <param name="category">The action category, e.g. Unit, Buff or Field.</param>
        /// <returns>The corresponding highlighter sprite, or <c>null</c> if not found.</returns>
        public Sprite GetHighlighterSprite(EActionCategory category)
        {
            if (categoryToHighlighterSprite.TryGetValue(category, out Sprite sprite))
                return sprite;

            CustomLogger.LogWarning($"No highlighter sprite found for category {category}.", this);
            return null;
        }
    }
}