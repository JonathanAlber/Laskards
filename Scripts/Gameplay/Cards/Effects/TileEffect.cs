using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Represents an effect that can be applied to a tile on the game board.
    /// </summary>
    public class TileEffect : Effect<Tile>
    {
        public TileEffect(Tile target, ETeam creatorTeam, EDurationType type, int duration, EffectData effectData)
            : base(target, creatorTeam, type, duration, effectData) { }

        /// <summary>
        /// Indicates whether this effect causes the tile to be occupied (e.g., blocking movement).
        /// </summary>
        public virtual bool OccupiesTile => false;
    }
}