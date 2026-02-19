using System.Collections.Generic;
using Gameplay.Boss;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;
using Gameplay.Units;
using Gameplay.Units.Data;
using Systems.Services;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Effect that damages the boss when a Type A card is played.
    /// </summary>
    public class DamageBossOnTypeAPlayEffect : PlayerEffect
    {
        private readonly int _damageAmount;

        public DamageBossOnTypeAPlayEffect(PlayerController target, ETeam creatorTeam, EDurationType durationType,
            int duration, EffectData effectData, int damageAmount)
            : base(target, creatorTeam, durationType, duration, effectData)
        {
            _damageAmount = damageAmount;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            Dictionary<string, object> map = new()
            {
                { "Duration", RemainingDuration },
                { "Damage", _damageAmount }
            };

            return map;
        }

        protected override void OnApply()
        {
            base.OnApply();
            PlayerController.OnCardPlayed += HandleCardPlayed;
        }

        protected override void OnRevert()
        {
            base.OnRevert();
            PlayerController.OnCardPlayed -= HandleCardPlayed;
        }

        private static bool ShouldTrigger(CardController card)
        {
            if (card == null)
                return false;

            return card is UnitCardController unitCard && unitCard.UnitData.UnitType == EUnitType.A;
        }

        private void HandleCardPlayed(CardController card)
        {
            if (!ShouldTrigger(card))
                return;

            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            boss.ApplyDamageWithDelay(_damageAmount, EDamageKind.PlayerEffect);
            RaiseDamagedBoss();
        }
    }
}