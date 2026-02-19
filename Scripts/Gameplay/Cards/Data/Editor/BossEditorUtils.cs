#if UNITY_EDITOR
using UnityEditor;

namespace Gameplay.Cards.Data.Editor
{
    /// <summary>
    /// Utility class for boss card editor functionalities.
    /// </summary>
    public static class BossEditorUtils
    {
        private static BossCardDefinitionCollection _cachedCollection;
        private static bool _hasTriedResolve;

        static BossEditorUtils() => EditorApplication.projectChanged += ClearCache;

        private static void ClearCache()
        {
            _cachedCollection = null;
            _hasTriedResolve = false;
        }

        /// <summary>
        /// Returns the <see cref="BossCardDefinitionCollection"/> using lazy caching.
        /// Refreshes only if project changes.
        /// </summary>
        public static BossCardDefinitionCollection FindBossCollection()
        {
            if (_cachedCollection != null)
                return _cachedCollection;

            if (_hasTriedResolve)
                return null;

            _hasTriedResolve = true;

            string[] guids = AssetDatabase.FindAssets("t:BossCardDefinitionCollection");

            if (guids == null || guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedCollection = AssetDatabase.LoadAssetAtPath<BossCardDefinitionCollection>(path);

            return _cachedCollection;
        }
    }
}
#endif