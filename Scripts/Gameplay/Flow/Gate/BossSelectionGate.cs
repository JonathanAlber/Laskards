using Utility.Logging;
using Gameplay.Boss.Data;

namespace Gameplay.Flow.Gate
{
    /// <summary>
    /// Gate participant that requires the player to select a boss before proceeding.
    /// </summary>
    public sealed class BossSelectionGate : GateParticipant
    {
        protected override string Reason => "Boss selection";

        protected override void Begin() => GameContextService.OnBossChosen += HandleBossChosen;

        protected override void End() => GameContextService.OnBossChosen -= HandleBossChosen;

        private void HandleBossChosen(BossData boss)
        {
            if (boss == null)
            {
                CustomLogger.LogWarning("Cannot complete boss selection gate, because null boss was chosen.", this);
                return;
            }

            Complete();
        }
    }
}