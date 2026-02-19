using Utility.Logging;
using Gameplay.StarterDecks.Data;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// Gate participant that requires the player to select a starter deck before proceeding.
    /// </summary>
    public sealed class StarterDeckSelectionGate : GateParticipant
    {
        protected override string Reason => "Starter deck selection";

        protected override void Begin() => GameContextService.OnStarterDeckChosen += HandleDeckChosen;

        protected override void End() => GameContextService.OnStarterDeckChosen -= HandleDeckChosen;

        private void HandleDeckChosen(StarterDeckDefinition deck)
        {
            if (deck == null)
            {
                CustomLogger.LogWarning("Cannot complete starter deck selection gate, because null deck was chosen.", this);
                return;
            }

            Complete();
        }
    }
}