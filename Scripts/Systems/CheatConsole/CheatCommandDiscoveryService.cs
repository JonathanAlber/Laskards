using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Handles discovery of cheat commands across assemblies and scene objects.
    /// </summary>
    public static class CheatCommandDiscoveryService
    {
        private static List<CheatCommandInfo> _cachedStaticCommands;

        /// <summary>
        /// Discovers all cheat commands available in the current context.
        /// </summary>
        public static List<CheatCommandInfo> DiscoverAllCheatCommands()
        {
            List<CheatCommandInfo> result = new();

            try
            {
                if (_cachedStaticCommands == null)
                {
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    _cachedStaticCommands = CheatCommandRegistry.CreateFromStaticMethods(new[] { executingAssembly });
                }

                result.AddRange(_cachedStaticCommands);

                MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour),
                    FindObjectsInactive.Include, FindObjectsSortMode.None) as MonoBehaviour[];

                result.AddRange(CheatCommandRegistry.CreateFromTargets(behaviours));
            }
            catch (Exception ex)
            {
                CustomLogger.LogWarning($"Failed to discover cheat commands: {ex}", null);
            }

            return result;
        }
    }
}