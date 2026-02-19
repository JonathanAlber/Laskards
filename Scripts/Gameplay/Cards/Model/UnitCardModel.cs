using System;
using System.Collections.Generic;
using Gameplay.Cards.Data;
using Gameplay.Cards.Data.States;
using Gameplay.StatLayers.Cards;
using Gameplay.Units.Data;

namespace Gameplay.Cards.Model
{
    /// <summary>
    /// Runtime model for a <see cref="UnitCardController"/>.
    /// Holds both the common data (from <see cref="CardModel"/>) and the unit-specific data.
    /// </summary>
    public sealed class UnitCardModel : CardModel, IStatLayerHost<IUnitCardStatLayer>
    {
        /// <summary>
        /// Invoked whenever any unit-specific data changes.
        /// </summary>
        public event Action OnUnitDataChanged;

        /// <summary>
        /// The unit-specific state of this card.
        /// </summary>
        public UnitCardState Unit { get; }

        /// <summary>
        /// The unit data associated with this unit card.
        /// </summary>
        public UnitData UnitData { get; }

        private readonly List<IUnitCardStatLayer> _unitLayers;

        IReadOnlyList<IUnitCardStatLayer> IStatLayerHost<IUnitCardStatLayer>.Layers => _unitLayers;

        public UnitCardModel(CardDefinition definition) : base(definition)
        {
            UnitCardDefinition unit = (UnitCardDefinition)definition;
            Unit = unit.UnitData.Unit.Clone();
            UnitData = unit.UnitData;
            _unitLayers = new List<IUnitCardStatLayer>();
        }

        void IStatLayerHost<IUnitCardStatLayer>.AddLayer(IUnitCardStatLayer layer)
        {
            _unitLayers.Add(layer);
            OnUnitDataChanged?.Invoke();
        }

        void IStatLayerHost<IUnitCardStatLayer>.RemoveLayer(IUnitCardStatLayer layer)
        {
            _unitLayers.Remove(layer);
            OnUnitDataChanged?.Invoke();
        }

        void IStatLayerHost<IUnitCardStatLayer>.ClearLayers()
        {
            _unitLayers.Clear();
            OnUnitDataChanged?.Invoke();
        }
    }
}