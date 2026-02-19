#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using Utility.Logging;

namespace Gameplay.StarterDecks.Data.Editor
{
    /// <summary>
    /// Utility methods for working with <see cref="StarterDeckCollection"/> assets in the Unity Editor.
    /// Uses lazy caching to avoid repeated asset database scans.
    /// </summary>
    public static class StarterDeckCollectionEditorUtils
    {
        private static StarterDeckCollection _cachedCollection;
        private static bool _hasTriedResolve;

        static StarterDeckCollectionEditorUtils() => EditorApplication.projectChanged += ClearCache;

        private static void ClearCache()
        {
            _cachedCollection = null;
            _hasTriedResolve = false;
        }

        /// <summary>
        /// Finds and returns the <see cref="StarterDeckCollection"/> using lazy caching.
        /// Logs warnings if none or multiple are found.
        /// </summary>
        public static StarterDeckCollection FindCollection()
        {
            if (_cachedCollection != null)
                return _cachedCollection;

            if (_hasTriedResolve)
                return null;

            _hasTriedResolve = true;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StarterDeckCollection)}");
            switch (guids.Length)
            {
                case 0:
                    CustomLogger.LogWarning($"No {nameof(StarterDeckCollection)} found. Please create one.", null);
                    return null;

                case > 1:
                    CustomLogger.LogWarning($"Multiple {nameof(StarterDeckCollection)} assets found. Using the first " +
                                            $"one at {AssetDatabase.GUIDToAssetPath(guids[0])}.", null);
                    break;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedCollection = AssetDatabase.LoadAssetAtPath<StarterDeckCollection>(path);

            if (_cachedCollection == null)
                CustomLogger.LogWarning($"Failed to load {nameof(StarterDeckCollection)} at '{path}'.", null);

            return _cachedCollection;
        }

        /// <summary>
        /// Adds the given <see cref="StarterDeckDefinition"/> to the collection.
        /// Prevents duplicate DisplayNames.
        /// </summary>
        public static void AddToCollection(
            StarterDeckDefinition deck,
            StarterDeckCollection collection)
        {
            if (deck == null || collection == null)
            {
                CustomLogger.LogWarning("Cannot add to collection: Deck or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (collection.Decks.Any(d => d.DisplayName == deck.DisplayName))
            {
                CustomLogger.LogWarning($"A deck with the DisplayName '{deck.DisplayName}' already exists " +
                                        $"in {collection.name}.", null);
                return;
            }

            if (collection.Decks.Contains(deck))
            {
                CustomLogger.Log($"'{deck.DisplayName}' is already part of {collection.name}.", null);
                return;
            }

            collection.Decks.Add(deck);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Added '{deck.DisplayName}' to {collection.name}.", null);
        }

        /// <summary>
        /// Removes the given <see cref="StarterDeckDefinition"/> from the collection.
        /// </summary>
        public static void RemoveFromCollection(StarterDeckDefinition deck, StarterDeckCollection collection)
        {
            if (deck == null || collection == null)
            {
                CustomLogger.LogWarning("Cannot remove from collection: Deck or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (!collection.Decks.Contains(deck))
            {
                CustomLogger.Log($"'{deck.DisplayName}' was not found in {collection.name}.", null);
                return;
            }

            collection.Decks.Remove(deck);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Removed '{deck.DisplayName}' from {collection.name}.", null);
        }

        /// <summary>
        /// Removes null or missing deck references from the collection.
        /// </summary>
        private static void CleanupNullReferences(StarterDeckCollection collection)
        {
            if (collection == null)
                return;

            int removedCount = collection.Decks.RemoveAll(d => d == null);
            if (removedCount <= 0)
                return;

            CustomLogger.Log($"Removed {removedCount} null reference{(removedCount == 1 ? "" : "s")} " +
                             $"from {collection.name}.", null);

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif