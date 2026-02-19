using Gameplay.CardExecution;
using Gameplay.CardExecution.Targeting;
using Gameplay.Cards.Data;
using Gameplay.Cards.Model;
using Gameplay.Cards.View;
using Gameplay.Units;
using Gameplay.Units.Data;
using UnityEngine;
using Utility.Logging;

namespace Gameplay.Cards
{
    /// <summary>
    /// Represents a unit card that can be played to summon units onto the board.
    /// </summary>
    public sealed class UnitCardController : CardController
    {
        [Header("Unit Card")]
        [field: Tooltip("View for this unit card.")]
        [field: SerializeField] public UnitCardView CardView { get; private set; }

        /// <summary>
        /// The unit data associated with this unit card.
        /// </summary>
        public UnitData UnitData { get; private set; }

        public override CardView View => CardView;

        public override CardModel Model => _typedModel;

        private UnitCardModel _typedModel;

        public override void Initialize(CardDefinition cardDefinition)
        {
            if (cardDefinition is UnitCardDefinition typed)
                Initialize(typed);
            else
                CustomLogger.LogWarning($"Tried to initialize {nameof(UnitCardController)} " +
                                        $"with non-{nameof(UnitCardDefinition)}.", this);
        }

        public override bool TryPlay(ICardPlayer player, ICardTargetResolver resolver, out string failReason)
        {
            if (!UnitCardExecutor.TryExecute(_typedModel, player, resolver, out failReason, out UnitController newUnit))
                return false;

            player.OnCardExecuted(this);
            player.OnNewUnitSpawned(newUnit);

            NotifyCardPlayed();

            return true;
        }

        private void Initialize(UnitCardDefinition definition)
        {
            _typedModel = new UnitCardModel(definition);
            CardView.Initialize(_typedModel, EActionCategory.Unit);
            CardView.SetBirdSprite(definition.UnitData.UnitType);
            UnitData = definition.UnitData;
        }
    }
}