using Gameplay.Cards.Data;

namespace Gameplay.Movement.AI
{
    /// <summary>
    /// Snapshot of a tile effect for AI evaluation purposes.
    /// </summary>
    public sealed class AiTileEffectSnapshot : AiEffectSnapshot<AiTileEffectSnapshot>
    {
        /// <summary>
        /// Indicates whether this effect blocks movement on the tile.
        /// </summary>
        public readonly bool BlocksTile;

        public AiTileEffectSnapshot(EDurationType durationType, int remainingDuration, bool blocksTile)
            : base(durationType, remainingDuration) => BlocksTile = blocksTile;

        public override AiTileEffectSnapshot Tick()
        {
            return DurationType != EDurationType.Temporary
                ? this
                : new AiTileEffectSnapshot(DurationType, RemainingDuration - 1, BlocksTile);
        }
    }
}