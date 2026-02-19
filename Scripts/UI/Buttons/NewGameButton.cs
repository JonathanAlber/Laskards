using Gameplay.Flow;
using Gameplay.Flow.Data;
using Systems.Services;
using UI.Confirmation;

namespace UI.Buttons
{
    /// <summary>
    /// Button that starts a new game with the currently selected boss and starter deck.
    /// </summary>
    public class NewGameButton : ConfirmedLoadSceneButton
    {
        protected override void OnClick()
        {
            if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                return;

            gameContextService.PrepareRun(ERunStartMode.NewRun);

            base.OnClick();
        }
    }
}