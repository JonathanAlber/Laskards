using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.StatLayers.Units;
using Gameplay.Units;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Base effect that applies a stat layer to a unit.
    /// </summary>
    public class UnitEffectWithLayer : UnitEffect
    {
        public readonly IUnitStatLayer StatLayer;

        protected UnitEffectWithLayer(UnitController target, ETeam creatorTeam, EDurationType durationType, int duration,
            EffectData effectData, IUnitStatLayer layer)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            StatLayer = layer;
        }

        protected override void OnApply() => Target.Model.AddLayer(StatLayer);

        protected override void OnRevert() => Target.Model.RemoveLayer(StatLayer);
    }
}