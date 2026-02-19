using System;
using System.Threading.Tasks;
using Attributes;
using UnityEngine;
using Gameplay.Flow;
using Gameplay.Flow.Data;
using SceneManagement;
using Utility.Logging;
using Systems.Services;
using UI.Confirmation;

namespace UI.Buttons
{
    /// <summary>
    /// Button that retries the game with the same boss and starter deck.
    /// </summary>
    public class RetryGameButton : BaseConfirmationButton
    {
        [SceneName, SerializeField] private string gameplaySceneName;

        protected override void OnClick() => ShowConfirmationBox();

        protected override void OnConfirm() => _ = LoadScene();

        private async Task LoadScene()
        {
            try
            {
                if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                    return;

                if (gameContextService.SelectedBoss == null || gameContextService.SelectedStarterDeck == null)
                    return;

                gameContextService.PrepareRun(ERunStartMode.Retry);

                if(ServiceLocator.TryGet(out SceneLoadingManager sceneLoader))
                    await sceneLoader.LoadSceneAsync(gameplaySceneName);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Ran into an error {e}, while loading the scene {gameplaySceneName}.", this);
            }
        }
    }
}