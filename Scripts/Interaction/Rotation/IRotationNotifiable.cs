using UnityEngine;

namespace Interaction.Rotation
{
    /// <summary>
    /// Receives notifications when a transform's rotation changes during a tween.
    /// </summary>
    public interface IRotationNotifiable
    {
        /// <summary>
        /// Called when the observed transform's rotation changes.
        /// </summary>
        /// <param name="newRotation">The current rotation.</param>
        void OnRotationChanged(Quaternion newRotation);
    }
}