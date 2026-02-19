using Systems.Services;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Provides a global reference to the main world-space canvas used for UI dragging.
    /// </summary>
    public sealed class WorldCanvasProvider : GameServiceBehaviour
    {
        /// <summary>
        /// The world-space canvas.
        /// </summary>
        [field: SerializeField] public Canvas WorldCanvas { get; private set; }
    }
}