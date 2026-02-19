using Gameplay.CardExecution;
using Gameplay.Cards.Effects;
using Gameplay.Cards.Modifier.Data.Modifiers;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using Gameplay.Player;
using Systems.Services;

namespace Gameplay.Cards.Modifier.Data.Runtime
{
    /// <summary>
    /// Runtime state for the Draw Phase Modifier.
    /// </summary>
    public class DrawPhaseModifierRuntimeState : TypedModifierRuntimeState<DrawPhaseModifierData, PlayerController>
    {
        private readonly int _cardsToDraw;

        public DrawPhaseModifierRuntimeState(DrawPhaseModifierData data) : base(data) => _cardsToDraw = data.CardsToDraw;

        protected override IEffect CreateEffect(PlayerController target, ETeam creatorTeam)
        {
            return new PlayerEffect(target, creatorTeam, DurationType, Duration, EffectData);
        }

        public override void Execute()
        {
            if (!ServiceLocator.TryGet(out GameFlowSystem gameFlowSystem))
                return;

            if (!ServiceLocator.TryGet(out PlayerController playerState))
                return;

            gameFlowSystem.ForcePhaseChange(EGamePhase.PlayerDraw);

            PlayerSnapshot updatedPlayerSnapshot = playerState.Snapshot.WithCardsToDrawRemaining(_cardsToDraw);
            playerState.ApplySnapshot(updatedPlayerSnapshot);
        }
    }
}