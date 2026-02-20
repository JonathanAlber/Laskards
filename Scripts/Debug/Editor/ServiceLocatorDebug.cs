#if UNITY_EDITOR
using Systems.Services;
using UnityEditor;

namespace Debug.Editor
{
    /// <summary>
    /// Validates registered services in the <see cref="ServiceLocator"/> when exiting play mode.
    /// </summary>
    [InitializeOnLoad]
    public static class ServiceLocatorDebug
    {
        static ServiceLocatorDebug() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.delayCall += ServiceLocator.ValidateServices;
        }
    }
}
#endif