#if UNITY_EDITOR
using UnityEditor;
using Utility.Logging;

namespace Gameplay.Boss.Data.Editor
{
    /// <summary>
    /// Utility methods for working with <see cref="BossDataCollection"/> assets in the Unity Editor.
    /// Uses lazy caching to avoid repeated asset database scans.
    /// </summary>
    public static class BossDataCollectionEditorUtils
    {
        private static BossDataCollection _cachedCollection;
        private static bool _hasTriedResolve;

        static BossDataCollectionEditorUtils() => EditorApplication.projectChanged += ClearCache;

        private static void ClearCache()
        {
            _cachedCollection = null;
            _hasTriedResolve = false;
        }

        /// <summary>
        /// Finds and returns the <see cref="BossDataCollection"/> using lazy caching.
        /// Logs warnings if none or multiple are found.
        /// </summary>
        public static BossDataCollection FindCollection()
        {
            if (_cachedCollection != null)
                return _cachedCollection;

            if (_hasTriedResolve)
                return null;

            _hasTriedResolve = true;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BossDataCollection)}");
            switch (guids.Length)
            {
                case 0:
                    CustomLogger.LogWarning($"No {nameof(BossDataCollection)} found. Please create one.", null);
                    return null;

                case > 1:
                    CustomLogger.LogWarning($"Multiple {nameof(BossDataCollection)} assets found. Using the first one " +
                                            $"at {AssetDatabase.GUIDToAssetPath(guids[0])}.", null);
                    break;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedCollection =
                AssetDatabase.LoadAssetAtPath<BossDataCollection>(path);

            if (_cachedCollection == null)
                CustomLogger.LogWarning($"Failed to load {nameof(BossDataCollection)} at '{path}'.", null);

            return _cachedCollection;
        }

        /// <summary>
        /// Adds the specified <see cref="BossData"/> to the given <see cref="BossDataCollection"/>.
        /// </summary>
        public static void AddToCollection(BossData bossData, BossDataCollection collection)
        {
            if (collection == null || bossData == null)
            {
                CustomLogger.LogWarning("Cannot add to collection: BossData or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (collection.BossData.Contains(bossData))
            {
                CustomLogger.Log($"'{bossData.name}' is already part of {collection.name}.", null);
                return;
            }

            collection.BossData.Add(bossData);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Added '{bossData.name}' to {collection.name}.", null);
        }

        /// <summary>
        /// Removes the specified <see cref="BossData"/> from the given <see cref="BossDataCollection"/>.
        /// </summary>
        public static void RemoveFromCollection(BossData bossData, BossDataCollection collection)
        {
            if (collection == null || bossData == null)
            {
                CustomLogger.LogWarning("Cannot remove from collection: BossData or Collection is null.", null);
                return;
            }

            CleanupNullReferences(collection);

            if (!collection.BossData.Contains(bossData))
            {
                CustomLogger.Log($"'{bossData.name}' was not found in {collection.name}.", null);
                return;
            }

            collection.BossData.Remove(bossData);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            CustomLogger.Log($"Removed '{bossData.name}' from {collection.name}.", null);
        }

        /// <summary>
        /// Removes null or missing boss references from the given <see cref="BossDataCollection"/>.
        /// </summary>
        private static void CleanupNullReferences(BossDataCollection collection)
        {
            if (collection == null)
                return;

            int removedCount = collection.BossData.RemoveAll(b => b == null);
            if (removedCount <= 0)
                return;

            CustomLogger.Log($"Removed {removedCount} null reference{(removedCount == 1 ? "" : "s")} from {collection.name}.",
                null);

            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif