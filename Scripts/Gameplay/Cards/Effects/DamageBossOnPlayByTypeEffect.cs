using System.Collections.Generic;
using Gameplay.Boss;
using Gameplay.CardExecution;
using Gameplay.Cards.Data;
using Gameplay.Cards.Effects.Display;
using Gameplay.Player;
using Gameplay.Units;
using Gameplay.Units.Data;
using Systems.Services;
using Utility.Logging;

namespace Gameplay.Cards.Effects
{
    /// <summary>
    /// Effect that damages the boss when the player plays a card of a specific action type.
    /// </summary>
    public class DamageBossOnPlayByTypeEffect : PlayerEffect
    {
        private readonly EActionCategory _actionCategory;
        private readonly int _damageAmount;

        public DamageBossOnPlayByTypeEffect(PlayerController target, ETeam creatorTeam, EDurationType durationType,
            int duration, EffectData effectData, EActionCategory actionCategory,
            int damageAmount) : base(target, creatorTeam, durationType, duration, effectData)
        {
            _actionCategory = actionCategory;
            _damageAmount = damageAmount;
        }

        public override IReadOnlyDictionary<string, object> GetTooltipValues()
        {
            Dictionary<string, object> map = new()
            {
                { "Duration", RemainingDuration },
                { "Damage", _damageAmount },
                { "ActionType", ActionCategoryUtility.GetActionCategoryName(_actionCategory) }
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

        private void HandleCardPlayed(CardController card)
        {
            if (!ShouldTrigger(card))
                return;

            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            boss.ApplyDamageWithDelay(_damageAmount, EDamageKind.PlayerEffect);
            RaiseDamagedBoss();
        }

        private bool ShouldTrigger(CardController card)
        {
            switch (card)
            {
                case ActionCardController actionCard:
                {
                    return _actionCategory == actionCard.TypedModel.ModifierState.ActionCategory;
                }
                case UnitCardController unitCard:
                {
                    if (_actionCategory != EActionCategory.Unit)
                        return false;

                    // Only deal damage for special units
                    switch (unitCard.UnitData.UnitType)
                    {
                        case EUnitType.B:
                        case EUnitType.C:
                        case EUnitType.D:
                        case EUnitType.E:
                            return true;
                        case EUnitType.A:
                        default:
                            return false;
                    }
                }
                default:
                {
                    CustomLogger.LogWarning("Received unsupported card type in" +
                                            $" {nameof(DamageBossOnPlayByTypeEffect)}.", null);
                    return false;
                }
            }
        }
    }
}