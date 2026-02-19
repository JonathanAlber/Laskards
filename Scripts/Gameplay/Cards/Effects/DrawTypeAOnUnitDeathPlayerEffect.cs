using System.Collections.Generic;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Decks.Controller;
using Gameplay.Player;
using Gameplay.Units;
using Systems.Services;
using Utility;
using Utility.Logging;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Player effect that draws Type A cards when any unit dies.
    /// </summary>
    public class DrawTypeAOnUnitDeathPlayerEffect : PlayerEffect
    {
        private readonly int _amountToDraw;

        public DrawTypeAOnUnitDeathPlayerEffect(PlayerController target, ETeam creatorTeam,
            EDurationType durationType, int duration, EffectData effectData, int amountToDraw)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            _amountToDraw = amountToDraw;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            return new Dictionary<string, object>
            {
                { "AmountToDraw", _amountToDraw },
                { "Duration", RemainingDuration }
            };
        }

        protected override void OnApply()
        {
            base.OnApply();

            UnitManager.OnPlayerUnitDied += HandlePlayerUnitDied;
        }

        protected override void OnRevert()
        {
            base.OnRevert();

            UnitManager.OnPlayerUnitDied -= HandlePlayerUnitDied;
        }

        private void HandlePlayerUnitDied(UnitController unit)
        {
            if (!ServiceLocator.TryGet(out UnitCardDeckController unitCardDeckController))
                return;

            // Give unit card one frame to add itself to the discard pile to ensure correct recycling behavior.
            CoroutineRunner.Instance.RunNextFrame(() =>
            {
                if (!unitCardDeckController.TryDraw(_amountToDraw))
                    CustomLogger.LogWarning("Failed to draw cards from Unit Deck.", null);
            });
        }
    }
}