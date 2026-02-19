using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the <see cref="BonusMovesModifierData"/>.
    /// </summary>
    public class BonusMovesModifierRuntimeState : TypedModifierRuntimeState<BonusMovesModifierData, UnitController>
    {
        private readonly int _bonusMoves;

        public BonusMovesModifierRuntimeState(BonusMovesModifierData data, int bonusMoves) : base(data)
        {
            _bonusMoves = bonusMoves;
        }

        protected override IEffect CreateEffect(UnitController target, ETeam creatorTeam)
        {
            return new BonusMovesUnitEffect(target, creatorTeam, DurationType, Duration, EffectData,
                new AddMovesLayer(_bonusMoves), _bonusMoves);
        }
    }
}