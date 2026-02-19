using System.Collections.Generic;

namespace Gameplay.StatLayers.Cards
{
    /// <summary>
    /// Contract for any model that maintains a set of stat layers of a specific type.
    /// Implementors must support adding, removing and clearing layers.
    /// </summary>
    /// <typeparam name="TLayer">A card stat layer type.</typeparam>
    public interface IStatLayerHost<TLayer> where TLayer : IStatLayer
    {
        /// <summary>
        /// All active layers of this type.
        /// </summary>
        IReadOnlyList<TLayer> Layers { get; }

        /// <summary>
        /// Adds a layer to the host.
        /// </summary>
        void AddLayer(TLayer layer);

        /// <summary>
        /// Removes a specific layer from the host.
        /// </summary>
        void RemoveLayer(TLayer layer);

        /// <summary>
        /// Removes all layers from the host.
        /// </summary>
        void ClearLayers();
    }
}