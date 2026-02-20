#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Gameplay.Cards.Data;
using Gameplay.Cards.Data.Editor;
using UnityEditor;
using Utility.Logging;

namespace Gameplay.StarterDecks.Data.Editor
{
    /// <summary>
    /// Utility methods for working with <see cref="StarterDeckDefinition"/> assets in the Unity Editor.
    /// </summary>
    public static class StarterDeckEditorUtils
    {
        private static List<StarterDeckDefinition> _cachedDecks;
        private static bool _hasTriedResolve;

        static StarterDeckEditorUtils() => EditorApplication.projectChanged += ClearCache;

        private static void ClearCache()
        {
            _cachedDecks = null;
            _hasTriedResolve = false;
        }

        /// <summary>
        /// Finds all <see cref="StarterDeckDefinition"/> assets in the project using lazy caching.
        /// </summary>
        public static List<StarterDeckDefinition> FindAllStarterDecks()
        {
            if (_cachedDecks != null)
                return _cachedDecks;

            if (_hasTriedResolve)
                return new List<StarterDeckDefinition>();

            _hasTriedResolve = true;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StarterDeckDefinition)}");
            _cachedDecks = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<StarterDeckDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid)))
                .Where(deck => deck != null)
                .ToList();

            if (_cachedDecks.Count == 0)
                CustomLogger.LogWarning("No StarterDeckData assets found in project.", null);

            return _cachedDecks;
        }

        /// <summary>
        /// Adds the specified <see cref="ActionCardDefinition"/> to the given <see cref="StarterDeckDefinition"/>.
        /// </summary>
        public static void AddToDeck(ActionCardDefinition card, StarterDeckDefinition deck)
        {
            if (deck == null || card == null)
            {
                CustomLogger.LogWarning("Cannot add to deck: Card or Deck is null.", null);
                return;
            }

            CleanupNullReferences(deck);

            if (deck.Cards.Contains(card))
            {
                CustomLogger.Log($"'{card.name}' is already part of {deck.name}.", null);
                return;
            }

            deck.Cards.Add(card);
            EditorUtility.SetDirty(deck);
            AssetDatabase.SaveAssets();
            CustomLogger.Log($"Added '{card.name}' to {deck.name}.", null);

            PlayerCardDefinitionCollection collection = PlayerCardDefinitionCollectionEditorUtils.FindCollection();
            if (collection != null && !collection.Cards.Contains(card))
                PlayerCardDefinitionCollectionEditorUtils.AddToCollection(card, collection);
        }

        /// <summary>
        /// Removes the specified <see cref="ActionCardDefinition"/> from the given <see cref="StarterDeckDefinition"/>.
        /// </summary>
        public static void RemoveFromDeck(ActionCardDefinition card, StarterDeckDefinition deck)
        {
            if (deck == null || card == null)
            {
                CustomLogger.LogWarning("Cannot remove from deck: Card or Deck is null.", null);
                return;
            }

            CleanupNullReferences(deck);

            if (!deck.Cards.Contains(card))
            {
                CustomLogger.Log($"'{card.name}' is not part of {deck.name}.", null);
                return;
            }

            deck.Cards.Remove(card);
            EditorUtility.SetDirty(deck);
            AssetDatabase.SaveAssets();
            CustomLogger.Log($"Removed '{card.name}' from {deck.name}.", null);
        }

        private static void CleanupNullReferences(StarterDeckDefinition deck)
        {
            if (deck == null)
                return;

            int removedCount = deck.Cards.RemoveAll(c => c == null);
            if (removedCount <= 0)
                return;

            CustomLogger.Log($"Removed {removedCount} null references from {deck.name}.", null);
            EditorUtility.SetDirty(deck);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif