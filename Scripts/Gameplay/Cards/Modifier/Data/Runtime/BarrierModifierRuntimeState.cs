using Gameplay.Board;
using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="BarrierModifierData"/>.
    /// </summary>
    public class BarrierModifierRuntimeState : TypedModifierRuntimeState<BarrierModifierData, Tile>
    {
        public BarrierModifierRuntimeState(BarrierModifierData data) : base(data) { }

        protected override IEffect CreateEffect(Tile target, ETeam creatorTeam)
        {
            return new BarrierTileEffect(target, creatorTeam, DurationType, Duration, EffectData);
        }
    }
}