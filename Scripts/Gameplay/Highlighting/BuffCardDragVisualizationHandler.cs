using Gameplay.Cards;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;
using Gameplay.Units;
using Systems.Services;
using Utility.Logging;
using Utility.Raycasting;

namespace Gameplay.Highlighting
{
    /// <summary>
    /// Handles visualization for dragging buff action cards, such as highlighting valid target units.
    /// </summary>
    public class BuffCardDragVisualizationHandler : ActionCardDragHighlighter<UnitController>
    {
        private UnitController _hoveredUnit;

        protected override void HandleDragStarted(CardController card)
        {
            ClearAllHighlights();
            _hoveredUnit = null;

            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out UnitManager unitManager))
                return;

            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            if (!player.HasEnoughEnergy(card.Model))
                return;

            card.View.FadeCard(true);

            ValidModifiables.Clear();
            foreach (UnitController unit in unitManager.PlayerUnits)
            {
                if (unit == null)
                {
                    CustomLogger.LogWarning("Encountered null unit while gathering valid buff targets.", this);
                    continue;
                }

                if (!unit.Model.IsAlive)
                {
                    CustomLogger.LogWarning($"Unit {unit.name} is not alive and cannot be targeted.", unit);
                    continue;
                }

                if (unit.HasMaxEffectsReached())
                    continue;

                ValidModifiables.Add(unit);
            }

            foreach (UnitController unit in ValidModifiables)
                unit.SetHighlightEffect(EHighlightMode.ValidTarget);
        }

        protected override void HandleDragging(CardController card)
        {
            if (card == null)
            {
                CustomLogger.LogWarning("Dragged card is null.", this);
                return;
            }

            if (!RaycastUtility.TryGetUIElement(uiRaycaster, out UnitController unit) || unit == null
                || !ValidModifiables.Contains(unit))
            {
                if (_hoveredUnit == null)
                    return;

                _hoveredUnit.SetHighlightEffect(EHighlightMode.ValidTarget);
                _hoveredUnit = null;

                return;
            }

            if (_hoveredUnit == unit)
                return;

            if (!ServiceLocator.TryGet(out PlayerController player))
                return;

            if (!player.HasEnoughEnergy(card.Model))
                return;

            if (_hoveredUnit != null)
                _hoveredUnit.SetHighlightEffect(EHighlightMode.ValidTarget);

            _hoveredUnit = unit;
            _hoveredUnit.SetHighlightEffect(EHighlightMode.HoveredTarget);
        }

        protected override void HandleDragEnded(CardController card)
        {
            if (card != null)
                card.View.FadeCard(false);

            ClearAllHighlights();
            _hoveredUnit = null;
            ValidModifiables.Clear();
        }

        private void ClearAllHighlights()
        {
            foreach (UnitController unit in ValidModifiables)
                unit.SetHighlightEffect(EHighlightMode.None);
        }
    }
}