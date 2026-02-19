using Gameplay.Board;
using Gameplay.Cards.Model;
using Gameplay.Cards.Modifier;
using Utility.Logging;

namespace Gameplay.CardExecution.Targeting
{
    /// <summary>
    /// Resolver used exclusively for boss opening setups.
    /// It always resolves the target to the specific tile provided.
    /// </summary>
    public sealed class BossSetupTargetResolver : ICardTargetResolver
    {
        private readonly Tile _tile;

        public BossSetupTargetResolver(Tile tile) => _tile = tile;

        public bool TryResolveTarget(CardModel cardModel, out IModifiableBase target)
        {
            if (_tile == null)
            {
                CustomLogger.LogWarning("Boss setup resolver was given a null tile.", null);
                target = null;
                return false;
            }

            switch (cardModel)
            {
                // Unit cards: the tile itself
                case UnitCardModel:
                    target = _tile;
                    return true;
                // Action cards: resolve target as the unit on this tile
                case ActionCardModel:
                {
                    if (_tile.OccupyingUnit != null)
                    {
                        target = _tile.OccupyingUnit;
                        return true;
                    }

                    CustomLogger.LogWarning("Attempted to apply an action card on a tile with no unit during setup.", null);
                    target = null;
                    return false;
                }
            }

            CustomLogger.LogWarning("Unsupported card model in setup resolver.", null);
            target = null;
            return false;
        }
    }
}