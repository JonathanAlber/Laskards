#if UNITY_EDITOR
using UnityEditor;
using Utility.Logging;

namespace Gameplay.Cards.Data.Editor
{
    /// <summary>
    /// Utility methods for working with <see cref="BossCardDefinitionCollection"/> assets in the Unity Editor.
    /// </summary>
    public static class BossCardDefinitionCollectionEditorUtils
    {
        private static BossCardDefinitionCollection _cachedCollection;
        private static bool _hasTriedResolve;

        static BossCardDefinitionCollectionEditorUtils() => EditorApplication.projectChanged += ClearCache;

        /// <summary>
        /// Returns the <see cref="BossCardDefinitionCollection"/> using lazy caching.
        /// </summary>
        public static BossCardDefinitionCollection FindCollection()
        {
            if (_cachedCollection != null)
                return _cachedCollection;

            if (_hasTriedResolve)
                return null;

            _hasTriedResolve = true;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BossCardDefinitionCollection)}");
            switch (guids.Length)
            {
                case 0:
                    CustomLogger.LogWarning($"No {nameof(BossCardDefinitionCollection)} found." +
                                            " Please create one.", null);
                    return null;
                case > 1:
                    CustomLogger.LogWarning($"Multiple {nameof(BossCardDefinitionCollection)} assets found. Using " +
                                            $"the first one at {AssetDatabase.GUIDToAssetPath(guids[0])}.", null);
                    break;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedCollection = AssetDatabase.LoadAssetAtPath<BossCardDefinitionCollection>(path);

            if (_cachedCollection == null)
                CustomLogger.LogWarning($"Failed to load {nameof(BossCardDefinitionCollection)} at '{path}'.", null);

            return _cachedCollection;
        }

        /// <summary>
        /// Adds the specified <see cref="CardDefinition"/> to the given <see cref="BossCardDefinitionCollection"/>.
        /// </summary>
        public static void AddToCollection(CardDefinition card, BossCardDefinitionCollection collection)
        {
            if (collection == null || card == null)
            {
                CustomLogger.LogWarning("Cannot add to collection: Card or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (collection.Cards.Contains(card))
            {
                CustomLogger.Log($"'{card.name}' is already part of {collection.name}.", null);
                return;
            }

            collection.Cards.Add(card);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Added '{card.name}' to {collection.name}.", null);
        }

        /// <summary>
        /// Removes the specified <see cref="CardDefinition"/> from the given <see cref="BossCardDefinitionCollection"/>.
        /// </summary>
        public static void RemoveFromCollection(CardDefinition card, BossCardDefinitionCollection collection)
        {
            if (collection == null || card == null)
            {
                CustomLogger.LogWarning("Cannot remove from collection: Card or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (!collection.Cards.Contains(card))
            {
                CustomLogger.Log($"'{card.name}' was not found in {collection.name}.", null);
                return;
            }

            collection.Cards.Remove(card);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Removed '{card.name}' from {collection.name}.", null);
        }

        private static void CleanupNullReferences(BossCardDefinitionCollection collection)
        {
            if (collection == null)
                return;

            int removedCount = collection.Cards.RemoveAll(c => c == null);
            if (removedCount <= 0)
                return;

            CustomLogger.Log($"Removed {removedCount} null reference{(removedCount == 1 ? "" : "s")}" +
                             $" from {collection.name}.", null);

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }

        private static void ClearCache()
        {
            _cachedCollection = null;
            _hasTriedResolve = false;
        }
    }
}
#endif