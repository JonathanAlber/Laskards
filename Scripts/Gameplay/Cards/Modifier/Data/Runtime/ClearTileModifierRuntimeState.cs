using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="ClearTileModifierData"/>.
    /// Allows the player to select a tile and clears all effects on that tile.
    /// </summary>
    public class ClearTileModifierRuntimeState : TypedModifierRuntimeState<ClearTileModifierData, Tile>
    {
        private Tile _target;

        public ClearTileModifierRuntimeState(ClearTileModifierData data) : base(data) { }

        protected override IEffect CreateEffect(Tile target, ETeam creatorTeam)
        {
            _target = target;
            return new TileEffect(target, creatorTeam, DurationType, Duration, EffectData);
        }

        public override void Execute()
        {
            base.Execute();

            _target.ClearEffects();
        }
    }
}